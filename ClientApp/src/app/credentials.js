"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.CredentialToken = exports.Credentials = void 0;
var Credentials = /** @class */ (function () {
    function Credentials(email, password) {
        this.email = email;
        this.password = password;
    }
    return Credentials;
}());
exports.Credentials = Credentials;
var CredentialToken = /** @class */ (function () {
    function CredentialToken(role, token) {
        this.role = role;
        this.token = token;
    }
    return CredentialToken;
}());
exports.CredentialToken = CredentialToken;
//# sourceMappingURL=credentials.js.map