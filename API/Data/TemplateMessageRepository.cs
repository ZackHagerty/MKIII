// using System;
// using System.Collections.Generic;
// using System.Data;
// using System.Linq;
// using System.Threading.Tasks;
// using API.DTOs;
// using API.Entities;
// using API.Helpers;
// using API.Interfaces;
// using Microsoft.Data.SqlClient;
// using Microsoft.Extensions.Configuration;

// namespace API.Data
// {
//     public class TemplateMessageRepository: IMessageRepository
//     {
//         public readonly IConfiguration _configuration;

//         public TemplateMessageRepository(IConfiguration configuration)
//         {
//             _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
//         }

//         public void AddGroup(Group group) //Not finished
//         {
//             using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
//             connection.Open();
//             using var command = connection.CreateCommand();
//             command.CommandText = "dbo.AddGroup";
//             command.CommandType = CommandType.StoredProcedure;

//         }
//         public void AddMessage(Message message) //Not finished
//         {
//             using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
//             connection.Open();
//             using var command = connection.CreateCommand();
//             command.CommandText = "dbo.AddMessage";
//             command.CommandType = CommandType.StoredProcedure;
//             command.Parameters.AddWithValue("@Id", message.Id);
//             command.Parameters.AddWithValue("@SenderId", message.SenderId);
//             command.Parameters.AddWithValue("@SenderUsername", message.SenderUsername);
//             command.Parameters.AddWithValue("@RecipientId", message.RecipientId);
//             command.Parameters.AddWithValue("@RecipientUsername", message.RecipientUsername);
//             command.Parameters.AddWithValue("@Content", message.Content);
//             command.Parameters.AddWithValue("@DateRead", message.DateRead);
//             command.Parameters.AddWithValue("@MessageSent", message.MessageSent);
//             command.Parameters.AddWithValue("@SenderDeleted", message.SenderDeleted);
//             command.Parameters.AddWithValue("@RecipientDeleted", message.RecipientDeleted);    
//         }

//         public void DeleteMessage(Message message)
//         {
//             using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
//             connection.Open();
//             using var command = connection.CreateCommand();
//             command.CommandText = "dbo.DeleteMessage";
//             command.CommandType = CommandType.StoredProcedure;
//             command.Parameters.AddWithValue("@Id", message.Id);
//         }

//         public async Task<Connection> GetConnection(string connectionId) //Not finished
//         {
//             using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
//             connection.Open();
//             using var command = connection.CreateCommand();
//             command.CommandText = "dbo.GetConnection";
//             command.CommandType = CommandType.StoredProcedure;
//             command.Parameters.AddWithValue("@Id", connectionId);
//         }

//         public async Task<Group> GetGroupForConnection(string connectionId) //Not finished
//         {
//             throw new NotImplementedException();
//         }

//         public async Task<Message> GetMessage(int id)
//         {
//             using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
//             connection.Open();
//             using var command = connection.CreateCommand();
//             command.CommandText = "dbo.GetMessage";
//             command.CommandType = CommandType.StoredProcedure;
//             command.Parameters.AddWithValue("@Id", id);

//             using var reader = await command.ExecuteReaderAsync();
//             while (reader.Read())
//             {
//                 return new Message()
//                 {
//                     Id = reader.GetInt32("Id"),
//                     SenderId = reader.GetInt32("Id"),
//                     SenderUsername = reader.GetString("SenderUsername"),
//                     RecipientUsername = reader.GetString("RecipientUsername"),
//                     Content = reader.GetString("Content"),
//                     DateRead = reader.GetDateTime("DateRead"),
//                     MessageSent = reader.GetDateTime("MessageSent"),
//                     SenderDeleted = reader.GetBoolean("SenderDeleted"),
//                     RecipientDeleted = reader.GetBoolean("RecipientDeleted")
//                 };
//             }
            
//             return null;

//         }

//         public async Task<Group> GetMessageGroup(string groupName) //Not finished
//         {
//             throw new NotImplementedException();
//         }

//         public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams) //Not finished
//         {
//             throw new NotImplementedException();
//         }

//         public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUsername, string recipientUsername) 
//         {
//             using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
//             connection.Open();
//             using var command = connection.CreateCommand();
//             command.CommandText = "dbo.GetMessageThread";
//             command.CommandType = CommandType.StoredProcedure;
//             command.Parameters.AddWithValue("@Sender", currentUsername);
//             command.Parameters.AddWithValue("@Recipient", recipientUsername);

//             using var reader = await command.ExecuteReaderAsync();
//             var result = new List<MessageDTO>();
//             while (reader.Read())
//             {
//                 result.Add(new MessageDTO()
//                 {
//                     Id = reader.GetInt32("Id"),
//                     SenderId = reader.GetInt32("SenderId"),
//                     SenderUsername = reader.GetString("SenderUsername"),
//                     SenderPhotoUrl = reader.GetString("SenderPhotoUrl"),
//                     RecipientId = reader.GetInt32("RecipientId"),
//                     RecipientUsername = reader.GetString("RecipientUsername"),
//                     RecipientPhotoUrl = reader.GetString("RecipientPhotoUrl"),
//                     Content = reader.GetString("Content"),
//                     DateRead = reader.GetDateTime("DateRead"),
//                     MessageSent = reader.GetDateTime("MessageSent"),

//                     SenderDeleted = reader.GetBoolean("SenderDeleted"),

//                     RecipientDeleted = reader.GetBoolean("RecipientDeleted")
                    
//                 });
//             }

//             return result;

//         }

//         public void RemoveConnection(Connection clientConnection)
//         {
//             using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
//             connection.Open();
//             using var command = connection.CreateCommand();
//             command.CommandText = "dbo.DeleteConnection";
//             command.CommandType = CommandType.StoredProcedure;
//             command.Parameters.AddWithValue("@Id", clientConnection.ConnectionId);
//         }
//     }
// }