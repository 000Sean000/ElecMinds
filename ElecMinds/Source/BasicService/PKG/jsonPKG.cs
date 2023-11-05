using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace PKG
{
	public static class JsonPKG
	{
		// Save json object to json file (JObject => *.json)
		public static void SaveJsonObjectToFile(JObject jsonObject, string filePath)
		{
			File.WriteAllText(filePath, jsonObject.ToString());
			Logger.WriteLine("JSON object has been save to: " + filePath);
		}

		// Read json object from json file (*.json => JObject)
		public static JObject ReadJsonObjectFromFile(string filePath)
		{
			if (File.Exists(filePath))
			{
				string jsonString = File.ReadAllText(filePath);
				try
				{
					return JObject.Parse(jsonString);
				}
				catch 
				{ 
					throw new Exception("Parsing Error: " + filePath); 
				}
			}
			else
			{
				throw new FileNotFoundException("JSON file not exist");
			}
		}
		// JObject store a key value as a JToken,
		// we need to turn JToken to the data type of representing variable
		public static T JTokenToType<T>(JToken jtoken)
		{
			T value;
			if (typeof(T).IsPrimitive || typeof(T) == typeof(string) || typeof(T) == typeof(DateTime))
			{
				value = jtoken.Value<T>();
				// Process the value as needed
			}
			else
			{
				value = jtoken.ToObject<T>();
				// Process the value as needed
			}
			return value;
		}
		public static T GetJObject<T>(JObject obj, params string[] keys)
		{
			int len = keys.Length;
			for (int i = 0; i < len - 1; i++)
			{
				if (!obj.ContainsKey(keys[i]) || obj[keys[i]] == null)
				{
					throw new Exception("Get JObject value failed. (missing key)");
				}
				else
				{
					obj = obj[keys[i]].Value<JObject>();
				}
			}
			JToken jtoken = obj[keys[len - 1]];
			T value = JTokenToType<T>(jtoken);
			return value;
		}
		public static void SetJObject<T>(T value, JObject obj, params string[] keys)
		{
			int len = keys.Length;
			for (int i = 0; i < len - 1; i++)
			{
				if (!obj.ContainsKey(keys[i]) || obj[keys[i]] == null)
				{
					obj[keys[i]] = new JObject();
					obj = obj[keys[i]].ToObject<JObject>();
				}
				else
				{
					obj = obj[keys[i]].ToObject<JObject>();
				}
			}
			obj[keys[len - 1]] = JToken.FromObject(value);
		}
		
		public static void MaintainJObject(JObject obj, params string[] keys)
		{
			int len = keys.Length;
			if (len == 0) return;
			for (int i = 0; i < len - 1; i++)
			{
				if (!obj.ContainsKey(keys[i]) || obj[keys[i]] == null)
				{
					obj[keys[i]] = new JObject();
					obj = obj[keys[i]].ToObject<JObject>();
				}
				else
				{
					obj = obj[keys[i]].ToObject<JObject>();
				}
			}
			obj[keys[len - 1]] = JToken.FromObject(new JObject());
		}
		
		public static void MaintainJObject<T>(JObject obj, T? initValue, params string[] keys)
		{
			int len = keys.Length;
			if (len == 0) return;
			for (int i = 0; i < len - 1; i++)
			{
				if (!obj.ContainsKey(keys[i]) || obj[keys[i]] == null) // check reference type null
				{
					obj[keys[i]] = new JObject();
					obj = obj[keys[i]].ToObject<JObject>();
				}
				else
				{
					obj = obj[keys[i]].ToObject<JObject>();
				}
			}
			if (!obj.ContainsKey(keys[len - 1]) || obj[keys[len - 1]].Type == JTokenType.Null) // check value type null
			{
				obj[keys[len - 1]] = JToken.FromObject(initValue);
			}
			else
			{
				// obj[keys[len - 1]] has value,
				// don't overwrite the original value
			}
		}

		// Modify JObject ( insert & remove ) 
		public static void DemoModifyJsonObject(JObject jsonObject)
		{
			// insert a new key-value pair
			jsonObject["email"] = "john@example.com";

			// remove a key-value pair
			jsonObject.Remove("age");
		}
		public static void DemoJson_(string filePath = "data.json")
		{
			// creat a new JObject
			JObject jsonObject = new JObject();
			jsonObject["name"] = "John";
			jsonObject["age"] = 30;

			// Save json object to json file
			SaveJsonObjectToFile(jsonObject, filePath);

			// Read json object from json file
			JObject readJsonObject = ReadJsonObjectFromFile(filePath);
			Logger.WriteLine("read JObject:");
			Logger.WriteLine(readJsonObject);

			// modify json object
			DemoModifyJsonObject(readJsonObject);
			Logger.WriteLine("modified JObject:");
			Logger.WriteLine(readJsonObject);
		}

	}


}

