"use strict";
////export class UserForm {
////  constructor(
////    public email: string,
////    public password: string,
////    public role: string
////  ) { }
////}
Object.defineProperty(exports, "__esModule", { value: true });
exports.ChangePasswordForm = exports.Details = exports.Account = void 0;
var Account = /** @class */ (function () {
    function Account(email, role, name) {
        this.email = email;
        this.role = role;
        this.name = name;
    }
    return Account;
}());
exports.Account = Account;
var Details = /** @class */ (function () {
    function Details(address, phone, name) {
        this.address = address;
        this.phone = phone;
        this.name = name;
    }
    return Details;
}());
exports.Details = Details;
var ChangePasswordForm = /** @class */ (function () {
    function ChangePasswordForm(oldPassword, password) {
        this.oldPassword = oldPassword;
        this.password = password;
    }
    return ChangePasswordForm;
}());
exports.ChangePasswordForm = ChangePasswordForm;
//# sourceMappingURL=user.js.map