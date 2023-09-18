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

export class CredentialsBC {
  constructor(
    public email: string,
    public password: string,
    public orgname: string
  ) {}
}

export class CredentialBCToken {
  constructor(public success: string, public message: MessageBC) {}
}

export class MessageBC {
  constructor(public email: string, public token: string, public org: string) {}
}