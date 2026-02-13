export class CredentialsBC {
  constructor(
    public email: string,
    public password: string,
    public orgName: string
  ) { }
}

export class CredentialBCToken {
  constructor(
    public success: string,
    public message: { token: string }
  ) { }
}