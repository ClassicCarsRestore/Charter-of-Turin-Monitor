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
