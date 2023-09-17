public virtual BaseResponseView<string> UpdateRequestFileRequestReturnConfirm(long request_file_request_id)
        {
            using (var uow = uowProvider.CreateUnitOfWork())
            {
                var repo_request = uow.GetRepository<Save010RequestFileAndEvidenceRequest>(); // เก็บคำขอ // table2
                var repo_fileAll = uow.GetRepository<vSave010RequestFileAndEvidence>();  // เก็บแฟ้มทั้งหมด // table1 view
                var repo_save010 = uow.GetRepository<Save010>(); // table1
                
                Save010RequestFileAndEvidenceRequest request = repo_request.Query(x => 
                    x.id == request_file_request_id 
                ).FirstOrDefault();

                if(request == null){
                    return new BaseResponseView<string>() 
                    { 
                        is_error = true,
                        error_message = $"ไม่พบข้อมูลการยืมแฟ้ม",
                        data = ""
                    };
                }

                Save010 file = repo_save010.Query(x => x.request_number == request.request_no).FirstOrDefault();
                if (file == null){
                    return new BaseResponseView<string>() 
                    { 
                        is_error = true,
                        error_message = $"ไม่พบข้อมูลแฟ้ม",
                        data = ""
                    };                    
                }
                if(request.status != borrow_status.WAIT_RETURN && request.status != borrow_status.SENT){
                    return new BaseResponseView<string>() 
                    { 
                        is_error = true,
                        error_message = $"ไม่อยู่ในสถานะ เบิกแฟ้ม หรือ รอคืนแฟ้ม",
                        data = ""
                    };
                }
                request.status = borrow_status.RETURNED;
                request.confirm_return_date = _today; 
                repo_request.Update(request);
                //next id will be done on Return Confirm
                var has_next_queue = false;
                if (request.next_borrow_id != null)
                {
                    Save010RequestFileAndEvidenceRequest next_request = repo_request.Query(x => 
                        x.request_no == request.request_no && 
                        x.type == request_file_and_evidence_type.RequestFile &&
                        x.status == borrow_status.NEW && 
                        x.borrow_id == request.next_borrow_id
                    ).OrderBy(x=>x.id).FirstOrDefault();
                    if(next_request != null){
                        next_request.status = borrow_status.PENDING;
                        repo_request.Update(next_request);
                        has_next_queue = true;
                    }
                }
        
                if ((request.to_destroy ?? false) == true) // replace null by false
                {
                    file.request_file_status = request_file_status.WAIT_SEND_DESTROY;
                    file.request_file_expected_destroy_date = file.trademark_expired_date.Value.AddYears(10);
                    file.request_file_destroy_mode = FindDestroyMode (file.request_number);
                }
                else
                {
                    file.request_file_status = has_next_queue? request_file_status.PENDING : request_file_status.AVALIABLE;
                }
                file.request_file_borrow_id = null;
                file.request_file_borrow_name = "";
                file.request_file_location = request_file_department_name.FL7;
                file.request_file_loc_code = request_file_department.FL7;
                file.request_file_borrow_col_id = null;
                file.request_file_borrow_date = null;
                file.request_file_borrow_end_date = null;
                repo_save010.Update(file);
                uow.SaveChanges();
                UpdateAmountQueue(file.request_number);
                return new BaseResponseView<string>()  // Success
                { 
                    data = ""
                };
            }
        }