import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LoginModel } from '../models/login-model';
import { apiConstants } from '../Helpers/api-constants';
import { Observable } from 'rxjs';
import { RegisterModel } from '../models/register-model';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(
    private http :HttpClient
  ) { }

  loginuser(loginUserObj: LoginModel): Observable<any>{

    return this.http.post<any>(apiConstants.login_user_path, loginUserObj)
  }

  registeruser(registerUserObj: RegisterModel): Observable<any>{

    return this.http.post<any>(apiConstants.register_user_path, registerUserObj)
  }

  getallusers() : Observable<any> { 
    return this.http.get<any>(apiConstants.get_all_users);
  }

}
