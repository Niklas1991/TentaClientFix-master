using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TentaClient.Model;
using TentaClient.Model.Account;
using TentaClient.Model.Entities;

namespace TentaClient
{
	class Program
	{
		protected static readonly DataContext.DataContext _context = new DataContext.DataContext();
		protected static readonly string urlHttp = "http://localhost:5000/api/";
		protected static readonly string urlHttps = "https://localhost:5001/api/";

		static async Task Main(string[] args)
		{			
			bool isRunning = true;
			while (isRunning)
			{
				Console.WriteLine("[a] Input a to create a first user");
				Console.WriteLine("[b] Input b to run everything as the first user(He is admin). This will also create the rest of the users");
				Console.WriteLine("[c] Input c to run everything as VD");
				Console.WriteLine("[d] Input d to run everything as CountryManager");
				Console.WriteLine("[e] Input e to run everything as Employee");
				Console.WriteLine("[x] Input x to exit the program");
				string menuChoice = Console.ReadLine();
				switch (menuChoice)
				{
					case ("a"):
						{
							var response = await PostFirstUser("FirstUserIsAdmin1991", 1, "FirstUserIsAdmin1991@hotmail.com", "!Hejalla1234");
							Console.WriteLine(response.ToString());
							break;
						}
					case ("b"):
						{
							string userName = "FirstUserIsAdmin1991";
							string password = "!Hejalla1234";
							var userAuthentication = new AuthenticateLogin { UserName = userName, Password = password };
							var authResult = await AuthUser(userAuthentication);							
							await PostCM(authResult.JwtToken, authResult.RefreshToken, authResult.Expires);
							await PostVD(authResult.JwtToken, authResult.RefreshToken, authResult.Expires);
							await PostUser("SecondUserIsEmployeeOnly1991", 2, "SecondUserIsEmployeeOnly1991@hotmail.com", "!Hejalla1234", authResult.JwtToken, authResult.RefreshToken, authResult.Expires);
							await PostUser("UserToDelete", 3, "UserToDelete@hotmail.com", "!Hejalla1234", authResult.JwtToken, authResult.RefreshToken, authResult.Expires);
							await PostUser("UserToDeleteAsOther", 6, "UserToDeleteAsOther@hotmail.com", "!Hejalla1234", authResult.JwtToken, authResult.RefreshToken, authResult.Expires);
							Console.WriteLine("All users created! ");
							Thread.Sleep(1000);
							Console.WriteLine("Running GET ALL ORDERS (Should return everything) ");
							Thread.Sleep(1000);
							var getAllOrderList = await GetAllOrders(authResult.JwtToken, authResult.RefreshToken, authResult.Expires);
							foreach (var order in getAllOrderList)
							{
								Console.WriteLine(order.CustomerId + " " + order.EmployeeId + " " + order.ShipCountry);
							}
							Thread.Sleep(1000);
							Console.WriteLine("Running GET MY ORDERS on employeeID 7 (Should return everything from employeeID 7)");
							Thread.Sleep(1000);
							var getMyOrderList = await GetMyOrders(authResult.JwtToken, authResult.RefreshToken, authResult.Expires, 7);
							foreach (var order in getMyOrderList)
							{
								Console.WriteLine(order.CustomerId + " " + order.EmployeeId + " " + order.ShipCountry);
							}
							Thread.Sleep(1000);
							Console.WriteLine("Running GET COUNTRY ORDERS (Should return everything from country FRANCE)");
							Thread.Sleep(1000);
							var getCountryOrderLIst = await GetCountryOrders(authResult.JwtToken, authResult.RefreshToken, authResult.Expires, "FRANCE");
							foreach (var order in getCountryOrderLIst)
							{
								Console.WriteLine(order.CustomerId + " " + order.EmployeeId + " " + order.ShipCountry);
							}
							Thread.Sleep(1000);							
							Console.WriteLine("Trying to delete 'UserToDelete' account as Admin (Will return OK)");
							Thread.Sleep(1000);
							var deleteUserAsEmployeeResponse = await DeleteUser(authResult.JwtToken, "UserToDelete", authResult.RefreshToken,  authResult.Expires);
							Console.WriteLine(deleteUserAsEmployeeResponse);


							break;
						}
					case ("c"):
						{
							string userName = "VD1991";
							string password = "!Hejalla1234";
							var userAuthentication = new AuthenticateLogin { UserName = userName, Password = password };
							var authResult = await AuthUser(userAuthentication);
							
							Thread.Sleep(1000);
							Console.WriteLine("Running GET ALL ORDERS as VD (Should return everything) ");
							Thread.Sleep(1000);
							var getAllOrderList = await GetAllOrders(authResult.JwtToken, authResult.RefreshToken, authResult.Expires);
							foreach (var order in getAllOrderList)
							{
								Console.WriteLine(order.CustomerId + " " + order.EmployeeId + " " + order.ShipCountry);
							}
							Thread.Sleep(1000);
							Console.WriteLine("Running GET MY ORDERS as VD on employeeID 6 (Should return everything from employeeID 6)");
							Thread.Sleep(1000);
							var getMyOrderList = await GetMyOrders(authResult.JwtToken, authResult.RefreshToken, authResult.Expires, 6);
							foreach (var order in getMyOrderList)
							{
								Console.WriteLine(order.CustomerId + " " + order.EmployeeId + " " + order.ShipCountry);
							}
							Thread.Sleep(1000);
							Console.WriteLine("Running GET COUNTRY ORDERS (Should return everything from country GERMANY)");
							Thread.Sleep(1000);
							var getCountryOrderLIst = await GetCountryOrders(authResult.JwtToken, authResult.RefreshToken, authResult.Expires, "GERMANY");
							foreach (var order in getCountryOrderLIst)
							{
								Console.WriteLine(order.CustomerId + " " + order.EmployeeId + " " + order.ShipCountry);
							}
							Thread.Sleep(1000);
							Console.WriteLine("Trying to delete 'UserToDeleteAsOther' account as VD (Should not be able to so will return forbidden)");
							Thread.Sleep(1000);
							var deleteUserAsEmployeeResponse = await DeleteUser(authResult.JwtToken, "UserToDeleteAsOther", authResult.RefreshToken, authResult.Expires);
							Console.WriteLine(deleteUserAsEmployeeResponse);
							break;
						}
					case ("d"):
						{
							string userName = "CM1991";
							string password = "!Hejalla1234";
							var userAuthentication = new AuthenticateLogin { UserName = userName, Password = password };
							var authResult = await AuthUser(userAuthentication);
							
							Console.WriteLine("Running GET ALL ORDERS as Country Manager (Should return everything where shipcountry = employee.country) ");
							Thread.Sleep(1000);
							var getAllOrderList = await GetAllOrders(authResult.JwtToken, authResult.RefreshToken, authResult.Expires);
							foreach (var order in getAllOrderList)
							{
								Console.WriteLine(order.CustomerId + " " + order.EmployeeId + " " + order.ShipCountry);
							}
							Thread.Sleep(1000);
							Console.WriteLine("Running GET MY ORDERS as CM(Country manager has employee so returns everything on users employee)");
							Thread.Sleep(1000);
							var getMyOrderList = await GetMyOrders(authResult.JwtToken, authResult.RefreshToken, authResult.Expires);
							foreach (var order in getMyOrderList)
							{
								Console.WriteLine(order.CustomerId + " " + order.EmployeeId + " " + order.ShipCountry);
							}
							Thread.Sleep(1000);
							Console.WriteLine("Running GET COUNTRY ORDERS (Should return everything from own country)");
							Thread.Sleep(1000);
							var getCountryOrderLIst = await GetCountryOrders(authResult.JwtToken, authResult.RefreshToken, authResult.Expires);							
							foreach (var order in getCountryOrderLIst)
							{
								Console.WriteLine(order.CustomerId + " " + order.EmployeeId + " " + order.ShipCountry);
							}
							Thread.Sleep(1000);
							Console.WriteLine("Trying to delete 'UserToDeleteAsOther' account as CM (Should not be able to so will return forbidden)");
							Thread.Sleep(1000);
							var deleteUserAsEmployeeResponse = await DeleteUser(authResult.JwtToken, "UserToDeleteAsOther", authResult.RefreshToken, authResult.Expires);
							Console.WriteLine(deleteUserAsEmployeeResponse);

							break;
						}
					case ("e"):
						{
							string userName = "SecondUserIsEmployeeOnly1991";
							string password = "!Hejalla1234";
							var userAuthentication = new AuthenticateLogin { UserName = userName, Password = password };
							var authResult = await AuthUser(userAuthentication);

							Console.WriteLine("Running GET ALL ORDERS as Employee (Should not be able to so will return forbidden) ");
							Thread.Sleep(1000);
							try
							{
								var getAllOrderList = await GetAllOrders(authResult.JwtToken, authResult.RefreshToken, authResult.Expires);
								if (getAllOrderList != null)
								{
									foreach (var order in getAllOrderList)
									{
										Console.WriteLine(order.CustomerId + " " + order.EmployeeId + " " + order.ShipCountry);
									}
								}
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex.Message);								
							}															
							Thread.Sleep(1000);
							Console.WriteLine("Running GET MY ORDERS as Employee(Should return orders from employee)");
							Thread.Sleep(1000);
							try
							{
								var getAllOrderList = await GetMyOrders(authResult.JwtToken, authResult.RefreshToken, authResult.Expires);
								if (getAllOrderList != null)
								{
									foreach (var order in getAllOrderList)
									{
										Console.WriteLine(order.CustomerId + " " + order.EmployeeId + " " + order.ShipCountry);
									}
								}
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex.Message);
							}
							Thread.Sleep(1000);
							Console.WriteLine("Running GET COUNTRY ORDERS as Employee(Should not be able to so will return forbidden)");
							Thread.Sleep(1000);
							try
							{
								var getAllOrderList = await GetCountryOrders(authResult.JwtToken, authResult.RefreshToken, authResult.Expires);
								if (getAllOrderList != null)
								{
									foreach (var order in getAllOrderList)
									{
										Console.WriteLine(order.CustomerId + " " + order.EmployeeId + " " + order.ShipCountry);
									}
								}
							}
							catch (Exception ex)
							{
								Console.WriteLine(ex.Message);
							}
							Thread.Sleep(1000); 
							Console.WriteLine("Trying to delete first account with Employee (Should not be able to so will return forbidden)");
							Thread.Sleep(1000);
							var deleteUserAsEmployeeResponse = await DeleteUser(authResult.JwtToken, "UserToDeleteAsOther", authResult.RefreshToken, authResult.Expires);
							Console.WriteLine(deleteUserAsEmployeeResponse);
							break;
						}
					case ("x"):
					default:
						{
							isRunning = false;
							break;
						}
				}
			}			
		
		}

		#region CreatingUsers
		public static async Task<string> PostFirstUser(string userName, int employeeID, string email, string password)
		{			
			var user = new RegisterUser() { UserName = userName, EmployeeId = employeeID, Email = email, Password = password };
			string endpoint = "user/register-employee";
			var json = JsonConvert.SerializeObject(user);
			var data = new StringContent(json, Encoding.UTF8, "application/json");
			using var client = new HttpClient();

			var response = await client.PostAsync(urlHttps + endpoint, data);
			if (response.StatusCode != HttpStatusCode.OK)
			{
				return response.StatusCode.ToString() + response.Content.ReadAsStringAsync().Result;
			}
			string result = response.Content.ReadAsStringAsync().Result;
			return result;
		}
		public static async Task<string> PostUser(string userName, int employeeID, string email, string password, string token, string refreshToken, DateTime expiryRefreshToken)
		{

			var validToken = await TokenValidation(token, refreshToken, expiryRefreshToken);

			var user = new RegisterUser() { UserName = userName, EmployeeId = employeeID, Email = email, Password = password };			
			string endpoint = "user/register-employee";
			var json = JsonConvert.SerializeObject(user);
			var data = new StringContent(json, Encoding.UTF8, "application/json");
			using var client = new HttpClient();

			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", validToken);
			var response = await client.PostAsync(urlHttps + endpoint, data);
			if (response.StatusCode != HttpStatusCode.OK)
			{
				return response.StatusCode.ToString() + response.Content.ReadAsStringAsync().Result;
			}
			
			string result = response.Content.ReadAsStringAsync().Result;
			return result;
		}
		public static async Task<string> PostAdmin(string token, string refreshToken, DateTime expiryRefreshToken)
		{
			var validToken = await TokenValidation(token, refreshToken, expiryRefreshToken);			

			var user = new RegisterUser();
			user.UserName = "Admin1991";
			user.EmployeeId = 3;
			user.Email = "Admin@hotmail1.com";
			user.Password = "!Hejalla1234";
			string endpoint = "user/register-admin";
			var json = JsonConvert.SerializeObject(user);
			var data = new StringContent(json, Encoding.UTF8, "application/json");
			using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", validToken);
			var response = await client.PostAsync(urlHttps + endpoint, data);
			if (response.StatusCode != HttpStatusCode.OK)
			{
				return response.StatusCode.ToString() + response.Content.ReadAsStringAsync().Result;
			}
			string result = response.Content.ReadAsStringAsync().Result;
			return result;
		}
		public static async Task<string> PostVD(string token, string refreshToken, DateTime expiryRefreshToken)
		{
			var validToken = await TokenValidation(token, refreshToken, expiryRefreshToken);
			var user = new RegisterUser();
			user.UserName = "VD1991";
			user.EmployeeId = 4;
			user.Email = "VD@hotmail1.com";
			user.Password = "!Hejalla1234";

			string endpoint = "user/register-vd";
			var json = JsonConvert.SerializeObject(user);
			var data = new StringContent(json, Encoding.UTF8, "application/json");

			using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", validToken);

			var response = await client.PostAsync(urlHttps + endpoint, data);
			if (response.StatusCode != HttpStatusCode.OK)
			{
				return response.StatusCode.ToString() + response.Content.ReadAsStringAsync().Result;
			}
			string result = response.Content.ReadAsStringAsync().Result;
			return result;
		}
		public static async Task<string> PostCM(string token, string refreshToken, DateTime expiryRefreshToken)
		{
			var validToken = await TokenValidation(token, refreshToken, expiryRefreshToken);

			//var account = _context.Accounts.FirstOrDefault(u => u.JwtToken == validToken);
			//refreshToken = account.RefreshToken;
			var user = new RegisterUser();
			user.UserName = "CM1991";
			user.EmployeeId = 5;
			user.Email = "CM@hotmail1.com";
			user.Password = "!Hejalla1234";

			string endpoint = "user/register-countrymanager";
			var json = JsonConvert.SerializeObject(user);
			var data = new StringContent(json, Encoding.UTF8, "application/json");

			using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", validToken);

			var response = await client.PostAsync(urlHttps + endpoint, data);
			if (response.StatusCode != HttpStatusCode.OK)
			{
				return response.StatusCode.ToString() + response.Content.ReadAsStringAsync().Result;
			}
			string result = response.Content.ReadAsStringAsync().Result;
			return result;
		}
		#endregion
		public static async Task<AuthenticateResponse> AuthUser(AuthenticateLogin user)
		{
			string endpoint = "user/authenticate";
			var json = JsonConvert.SerializeObject(user);
			var data = new StringContent(json, Encoding.UTF8, "application/json");
			using (var client = new HttpClient())
			{
				var response = await client.PostAsync(urlHttps + endpoint, data);
				var result = await response.Content.ReadAsStringAsync();
				var authResult = JsonConvert.DeserializeObject<AuthenticateResponse>(result);

				var findUser = await _context.Accounts.Where(x => x.UserName == user.UserName).FirstOrDefaultAsync();
				if (findUser == null)
				{
					var account = new Account { UserName = authResult.UserName, RefreshToken = authResult.RefreshToken, JwtToken = authResult.JwtToken, Expires = authResult.Expires };
					await _context.Accounts.AddAsync(account);
					await _context.SaveChangesAsync();
					return authResult;
				}
				findUser.JwtToken = authResult.JwtToken;
				findUser.RefreshToken = authResult.RefreshToken;
				findUser.Expires = authResult.Expires;

				await _context.SaveChangesAsync();
				return authResult;
			}
		}
		#region CRUD Users
		public static async Task<IEnumerable<AccountResponse>> GetAllUsers(string token, string refreshToken, DateTime expiryRefreshToken)
		{
			var validToken = await TokenValidation(token, refreshToken, expiryRefreshToken);
			string endpoint = "user/get-all-users";
			using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", validToken);			
			var response = await client.GetAsync(urlHttps + endpoint);
			if (response.StatusCode != HttpStatusCode.OK)
			{
				throw new Exception(response.StatusCode.ToString() + response.Content.ReadAsStringAsync().Result);
			}
			var result = response.Content.ReadAsStringAsync().Result;
			var desResult = JsonConvert.DeserializeObject<IEnumerable<AccountResponse>>(result);
			return desResult;
		}

		public static async Task<string> DeleteUser(string token, string userName, string refreshToken, DateTime expiryRefreshToken)
		{
			var validToken = await TokenValidation(token, refreshToken, expiryRefreshToken);

			UriBuilder uribuilder = new UriBuilder();
			uribuilder.Scheme = "https";
			uribuilder.Port = 5001;
			uribuilder.Path = "/api/user/delete";
			uribuilder.Query = "username=" + userName;
			Uri uri = uribuilder.Uri;

			using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", validToken);
			var response = await client.DeleteAsync(uri);
			if (response.StatusCode != HttpStatusCode.OK)
			{
				return response.StatusCode.ToString() + response.Content.ReadAsStringAsync().Result;
			}
			return response.StatusCode.ToString();
		}
		public static async Task<string> UpdateUser(string token, UpdateRequest user, string refreshToken, DateTime expiryRefreshToken)
		{
			var validToken = await TokenValidation(token, refreshToken, expiryRefreshToken);
			UriBuilder uribuilder = new UriBuilder();
			uribuilder.Scheme = "https";
			uribuilder.Port = 5001;
			uribuilder.Path = "/api/user/update-employee";
			uribuilder.Query = "username=" + user.UserName;
			Uri uri = uribuilder.Uri;
			var json = JsonConvert.SerializeObject(user);
			var data = new StringContent(json, Encoding.UTF8, "application/json");

			using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", validToken);
			var response = await client.PatchAsync(uri, data);
			return response.StatusCode.ToString();
		}

		#endregion

		#region CRUD Orders
		public static async Task<IEnumerable<OrderResponse>> GetAllOrders(string token, string refreshToken, DateTime expiryRefreshToken)
		{
			var validToken = await TokenValidation(token, refreshToken, expiryRefreshToken);
			string endpoint = "order/get-all-orders";
			using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", validToken);
			
			var response = await client.GetAsync(urlHttps + endpoint);
			if (response.StatusCode != HttpStatusCode.OK)
			{
				throw new Exception(response.StatusCode.ToString() + response.Content.ReadAsStringAsync().Result);
			}
			var result = response.Content.ReadAsStringAsync().Result;
			var desResult = JsonConvert.DeserializeObject<IEnumerable<OrderResponse>>(result);
			return desResult;
		}

		public static async Task<IEnumerable<OrderResponse>> GetMyOrders(string token, string refreshToken, DateTime expiryRefreshToken, int employeeId = 0)
		{
			var validToken = await TokenValidation(token, refreshToken, expiryRefreshToken);
			UriBuilder uribuilder = new UriBuilder();
			uribuilder.Scheme = "https";
			uribuilder.Port = 5001;
			uribuilder.Path = "/api/order/get-my-orders";
			uribuilder.Query = "employeeId=" + employeeId;
			Uri uri = uribuilder.Uri;
			using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", validToken);			
						
			var response = await client.GetAsync(uri);
			if (response.StatusCode != HttpStatusCode.OK)
			{
				throw new Exception(response.StatusCode.ToString() + response.Content.ReadAsStringAsync().Result);
			}
			var result = response.Content.ReadAsStringAsync().Result;
			var desResult = JsonConvert.DeserializeObject<IEnumerable<OrderResponse>>(result);
			return desResult;
		}
		public static async Task<IEnumerable<OrderResponse>> GetCountryOrders(string token, string refreshToken, DateTime expiryRefreshToken, string country = null)
		{
			var validToken = await TokenValidation(token, refreshToken, expiryRefreshToken);

			UriBuilder uribuilder = new UriBuilder();
			uribuilder.Scheme = "https";
			uribuilder.Port = 5001;
			uribuilder.Path = "/api/order/get-country-orders";
			uribuilder.Query = "country=" + country;
			Uri uri = uribuilder.Uri;

			using var client = new HttpClient();
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", validToken);			
			
			var response = await client.GetAsync(uri);
			if (response.StatusCode != HttpStatusCode.OK)
			{
				throw new Exception(response.StatusCode.ToString() + response.Content.ReadAsStringAsync().Result);
			}
			var result = response.Content.ReadAsStringAsync().Result;
			var desResult = JsonConvert.DeserializeObject<IEnumerable<OrderResponse>>(result);
			return desResult;			
		}
		#endregion

		public static async Task<string> TokenValidation(string jwtToken, string refreshToken, DateTime expiryRefreshToken)
		{
			var account = _context.Accounts.FirstOrDefault(u => u.RefreshToken == refreshToken);
			if (account != null)
			{
				if (expiryRefreshToken < DateTime.UtcNow)
				{
					var newRefreshToken = await RefreshTokenAsync(jwtToken, refreshToken);
					account.RefreshToken = newRefreshToken.RefreshToken;
					account.JwtToken = newRefreshToken.JwtToken;
					await _context.SaveChangesAsync();
					return account.JwtToken;
				}
				else
				{
					var handler = new JwtSecurityTokenHandler();
					var token = handler.ReadJwtToken(jwtToken);
					if (token.ValidTo <= DateTime.UtcNow)
					{
						var newTokenToReturn = await RefreshTokenAsync(jwtToken, refreshToken);
						account.RefreshToken = newTokenToReturn.RefreshToken;
						account.JwtToken = newTokenToReturn.JwtToken;
						await _context.SaveChangesAsync();
						return newTokenToReturn.JwtToken;
					}
					return jwtToken;
				}
			}
			else
				throw new System.ArgumentNullException("User does not exist with this refreshtoken, please login again.");
			
		}
		public static async Task<AuthenticateResponse> RefreshTokenAsync(string token, string refreshToken)
		{			
			string endpoint = "user/refresh-token";
			using var client = new HttpClient();	
			var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = refreshToken };
			var json = JsonConvert.SerializeObject(refreshTokenRequest);
			var data = new StringContent(json, Encoding.UTF8, "application/json");			
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			var response = await client.PostAsync(urlHttps + endpoint, data);
			string tokenResponse = response.Content.ReadAsStringAsync().Result;
			var result = JsonConvert.DeserializeObject<AuthenticateResponse>(tokenResponse);
			return result;
		}
	}
}

