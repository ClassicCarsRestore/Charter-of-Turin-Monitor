"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.Token = void 0;
var http_1 = require("@angular/common/http");
var Token = /** @class */ (function () {
    function Token() {
    }
    Token.getHeader = function () {
        var token = localStorage.getItem('RBToken');
        return { headers: new http_1.HttpHeaders().append('Authorization', "Bearer " + (token ? token : "")) };
    };
    return Token;
}());
exports.Token = Token;
//# sourceMappingURL=tokens.js.map