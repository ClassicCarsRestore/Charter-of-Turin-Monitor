db = db.getSiblingDB('TasklistDb');

db.createCollection('Credentials');

db.Credentials.insertOne(
	{
		username: "s3-processed-data-read-access-user",
		access_key_id: "your_access_key_id",
		secret_access_key: "your_secret_access_key"
	}
);

db.createCollection('LoginCredentials');

db.LoginCredentials.insertOne(
	{
		role: "admin",
		email: "AdminEmail",
		password: "AdminPassword"
	}
);