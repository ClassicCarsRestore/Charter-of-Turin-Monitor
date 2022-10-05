using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using tasklist.Models;

namespace tasklist.Services
{
	public class AmazonS3Service
	{
		private AmazonS3Client _client;
		private readonly IMongoCollection<Credentials> _credentials;

		public AmazonS3Service(ICredentialsDatabaseSettings settings)
		{
			var client = new MongoClient(settings.ConnectionString);
			var database = client.GetDatabase(settings.DatabaseName);

			_credentials = database.GetCollection<Credentials>(settings.CredentialsCollectionName);

			BasicAWSCredentials awsCreds = GetCredentialsFromDatabase();
			_client = new AmazonS3Client(awsCreds, RegionEndpoint.EUCentral1);
		}

		/// <summary>
		/// Auxiliary method that retrieves the credentials from Database to build and return the 
		/// BasicAWSCredentials object.
		/// </summary>
		/// <returns>a BasicAWSCredentials object with the credentials</returns>
		private BasicAWSCredentials GetCredentialsFromDatabase()
		{
			Credentials cred = _credentials.Find(credentials => true).ToList()[0];

			return new BasicAWSCredentials(cred.Access_key_id, cred.Secret_access_key);
		}

		/// <summary>
		/// Method that fetches the latest sensor information JSON file from AWS S3 and parses it to a 
		/// List of 'AmazonS3SensorInfo' objects. 
		/// 
		/// This method is based on the sample provided at:
		/// https://docs.aws.amazon.com/code-samples/latest/catalog/dotnetv3-S3-GetObjectExample-GetObjectExample-GetObject.cs.html
		/// </summary>
		/// <param name="folderName"></param>
		/// <param name="fileName"></param>
		/// <returns>a list containing the latest sensor information</returns>
		public async Task<List<AmazonS3SensorInfo>> GetSensorInformation(string folderName, string fileName)
		{
			string bucketName = "general-system-data";

			List<AmazonS3SensorInfo> list = new();

			try
			{
				// build the request object with the bucket name and key (path to file) defined in AWS
				GetObjectRequest request = new()
				{
					BucketName = bucketName,
					Key = "processed-data/" + folderName + "/" + fileName,
				};

				// processed-data/SensorBox_01/processed_SensorBox_01_data_07_09_2021_1.json

				using GetObjectResponse response = await _client.GetObjectAsync(request);

				// read the stream and convert it to our structures
				using StreamReader reader = new StreamReader(response.ResponseStream);

				list = JsonConvert.DeserializeObject<List<AmazonS3SensorInfo>>(reader.ReadToEnd()); //fakeInfo);

				return list;
			}
			catch (AmazonS3Exception ex)
			{
				Console.WriteLine($"Error encountered on server. Message:'{ex.Message}' getting list of objects.");
			}

			return list;
		}
	}
}
