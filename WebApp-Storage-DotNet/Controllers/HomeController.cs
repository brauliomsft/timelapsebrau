//---------------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved. 
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,  
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES  
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
//---------------------------------------------------------------------------------- 
// The example companies, organizations, products, domain names, 
// e-mail addresses, logos, people, places, and events depicted 
// herein are fictitious.  No association with any real company, 
// organization, product, domain name, email address, logo, person, 
// places, or events is intended or should be inferred. 

namespace WebApp_Storage_DotNet.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using System.Web;
    using System.Threading.Tasks;
    using System.IO;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.Azure;
    using System.Configuration;

    /// <summary> 
    /// Azure Blob Storage Photo Gallery - Demonstrates how to use the Blob Storage service.  
    /// Blob storage stores unstructured data such as text, binary data, documents or media files.  
    /// Blobs can be accessed from anywhere in the world via HTTP or HTTPS. 
    /// 
    /// Note: This sample uses the .NET 4.5 asynchronous programming model to demonstrate how to call the Storage Service using the  
    /// storage client libraries asynchronous API's. When used in real applications this approach enables you to improve the  
    /// responsiveness of your application. Calls to the storage service are prefixed by the await keyword.  
    ///  
    /// Documentation References:  
    /// - What is a Storage Account - http://azure.microsoft.com/en-us/documentation/articles/storage-whatis-account/ 
    /// - Getting Started with Blobs - http://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-blobs/ 
    /// - Blob Service Concepts - http://msdn.microsoft.com/en-us/library/dd179376.aspx  
    /// - Blob Service REST API - http://msdn.microsoft.com/en-us/library/dd135733.aspx 
    /// - Blob Service C# API - http://go.microsoft.com/fwlink/?LinkID=398944 
    /// - Delegating Access with Shared Access Signatures - http://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-shared-access-signature-part-1/ 
    /// </summary> 

    public class HomeController : Controller
    {
        static CloudBlobClient blobClient;
        const string blobContainerName = "photos";
        static CloudBlobContainer blobContainer;
		static BlobContinuationToken continuationToken = null;
		static int count = 1;

		/// <summary> 
		/// Task<ActionResult> Index() 
		/// Documentation References:  
		/// - What is a Storage Account: http://azure.microsoft.com/en-us/documentation/articles/storage-whatis-account/ 
		/// - Create a Storage Account: https://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-blobs/#create-an-azure-storage-account
		/// - Create a Storage Container: https://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-blobs/#create-a-container
		/// - List all Blobs in a Storage Container: https://azure.microsoft.com/en-us/documentation/articles/storage-dotnet-how-to-use-blobs/#list-the-blobs-in-a-container
		/// </summary> 
		public async Task<ActionResult> Index(int page = 1)
        {

			ViewBag.PageNumber = page;
			continuationToken = page==1 ?  null: continuationToken;
			List<Uri> blobs =await NextPage (page);
			return View (blobs);
		}

		public async Task<List<Uri>> NextPage(int page) {
			try {
				// Retrieve storage account information from connection string
				// How to create a storage connection string - http://msdn.microsoft.com/en-us/library/azure/ee758697.aspx
				CloudStorageAccount storageAccount = CloudStorageAccount.Parse (ConfigurationManager.AppSettings["StorageConnectionString"].ToString ());

				// Create a blob client for interacting with the blob service.
				blobClient = storageAccount.CreateCloudBlobClient ();
				blobContainer = blobClient.GetContainerReference (blobContainerName);
				await blobContainer.CreateIfNotExistsAsync ();

				// To view the uploaded blob in a browser, you have two options. The first option is to use a Shared Access Signature (SAS) token to delegate  
				// access to the resource. See the documentation links at the top for more information on SAS. The second approach is to set permissions  
				// to allow public access to blobs in this container. Comment the line below to not use this approach and to use SAS. Then you can view the image  
				// using: https://[InsertYourStorageAccountNameHere].blob.core.windows.net/webappstoragedotnet-imagecontainer/FileName 
				await blobContainer.SetPermissionsAsync (new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

				// Gets all Cloud Block Blobs in the blobContainerName and passes them to teh view
				List<Uri> allBlobs = new List<Uri> ();
				List<Uri> top100Blobs = new List<Uri> ();
				List<IListBlobItem> blobs = new List<IListBlobItem> ();

				BlobResultSegment listingResult = await blobContainer.ListBlobsSegmentedAsync (string.Empty, true, BlobListingDetails.Metadata, 50, continuationToken, null, null);
				continuationToken = listingResult.ContinuationToken;
				blobs.AddRange (listingResult.Results);
	

				foreach (IListBlobItem blob in blobs) {
					if (blob.GetType () == typeof (CloudBlockBlob))
						allBlobs.Add (blob.Uri);
				}
				ViewBag.AllBlobsCount = allBlobs.Count;
				return allBlobs;
			} catch (Exception ex) {
				ViewData["message"] = ex.Message;
				ViewData["trace"] = ex.StackTrace;
				return null;
			}
		}

		private string GetRandomBlobName(string filename)
        {
            string ext = Path.GetExtension(filename);
            return string.Format("{0:10}_{1}{2}", DateTime.Now.Ticks, Guid.NewGuid(), ext);
        }
    }
}
