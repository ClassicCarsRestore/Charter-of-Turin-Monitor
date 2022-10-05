////export class UserForm {
////  constructor(
////    public email: string,
////    public password: string,
////    public role: string
////  ) { }
////}

export class Account {
  constructor(
    public email: string,
    public role: string,
    public name: string
  ) { }
}

export class Details {
  constructor(
    public address: string,
    public phone: string,
    public name: string
  ) { }
}

export class ChangePasswordForm {
  constructor(
    public oldPassword: string,
    public password: string
  ) { }
}

