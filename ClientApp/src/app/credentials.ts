export class Credentials {
  constructor(
    public email: string,
    public password: string
  ) { }
}

export class CredentialToken {
  constructor(
    public role: string,
    public token: string
  ) { }
}
