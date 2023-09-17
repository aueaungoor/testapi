import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import {Book} from './book';
@Component({
  selector: 'app-testapi',
  templateUrl: './testapi.component.html',
  styleUrls: ['./testapi.component.css']
})
export class TestapiComponent implements OnInit{
  booklist:Book[] = [];

  constructor(private http: HttpClient ) {
    
  }

  ngOnInit(): void {
     this.http.get<Book[]>('http://localhost:5133/WeatherForecast')
    .subscribe(book => {
    
      this.booklist = book ;
      console.log(this.booklist);
    })
    
  }
  
  
  

  
}
