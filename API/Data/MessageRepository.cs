using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly DataContext _context;


        public MessageRepository(DataContext context, IConfiguration configuration, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        }

        public void AddGroup(Group group) //DONE
        {
            Console.WriteLine("Add Group");

            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.AddGroup";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Name", group.Name);
            
            command.ExecuteNonQuery();
           // _context.Groups.Add(group);
        }

        public void AddConnection(Connection connectionObject, string groupName) //DONE
        {

            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.AddConnection";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@ConnectionId", connectionObject.ConnectionId);
            command.Parameters.AddWithValue("@Username", connectionObject.Username);
            command.Parameters.AddWithValue("@GroupName", groupName);

            command.ExecuteNonQuery();
        }

        public void AddMessage(Message message) //DONE
        {

            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.AddMessage";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@SenderUsername", message.SenderUsername );
            command.Parameters.AddWithValue("@RecipientUsername", message.RecipientUsername );
            command.Parameters.AddWithValue("@Content", message.Content );                        
            command.Parameters.AddWithValue("@MessageSent", message.MessageSent );
            int bit = (message.RecipientDeleted == false) ? 0 : 1;
            command.Parameters.AddWithValue("@RecipientDeleted", message.RecipientDeleted );

            command.ExecuteNonQuery();

        }

        public void DeleteMessage(Message message) //DONE
        {
            Console.WriteLine("Delete Message Called");
            Console.WriteLine(message.Id);
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.DeleteMessage";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@MessageId", message.Id);

            command.ExecuteNonQuery();

        }

        public async Task<Connection> GetConnection(string connectionId) //no references
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.GetConnection";
            command.CommandType = CommandType.StoredProcedure;  
            command.Parameters.AddWithValue("@connectionId", connectionId);

            
            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                return new Connection{
                    ConnectionId = reader.GetString("ConnectionId"),
                    Username = reader.GetString("Username")
                };
            }

            return null;
        }

        public async Task<Group> GetGroupForConnection(string connectionId) //DONE
        {
            Console.WriteLine("GET GTOUP FOR CONNECTION");
            Console.WriteLine(connectionId);

            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.GetGroupForConnection";
            command.CommandType = CommandType.StoredProcedure;  
            command.Parameters.AddWithValue("@connectionId", connectionId);
 
            var connections = new List<Connection>();

            using var reader = await command.ExecuteReaderAsync();
            while(reader.Read())
            {

                connections.Add(new Connection
                {
                    ConnectionId = reader.GetString("ConnectionId"),
                    Username = reader.GetString("Username")
                });

                return new Group()
                {
                    Name = reader.GetString("GroupName"),
                    Connections = connections
                };
            }

            return null;
            // return await _context.Groups
            //     .Include(c => c.Connections)
            //     .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
            //     .FirstOrDefaultAsync();
        
        }

        public async Task<Message> GetMessage(int id) 
        {
            var sender = new AppUser();
            var recipient = new AppUser();
            Console.WriteLine("GET MESSAGE");
            Console.WriteLine(id);
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.GetMessage";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", id);

            using var reader = await command.ExecuteReaderAsync();
            
            while(reader.Read())
            {
                sender.DateOfBirth = reader.GetDateTime("DateOfBirth");
                sender.KnownAs = reader.GetString("KnownAs");
                sender.Created = reader.GetDateTime("Created");
                sender.LastActive = reader.GetDateTime("LastActive");
                sender.Gender = reader.GetString("Gender");
                sender.Introduction = (reader.IsDBNull("Introduction")) ? null : reader.GetString("Introduction");
                sender.LookingFor = (reader.IsDBNull("LookingFor")) ? null : reader.GetString("LookingFor");
                sender.Interests = (reader.IsDBNull("Interests")) ? null : reader.GetString("Interests");
                sender.City = reader.GetString("City");
                sender.Country = reader.GetString("Country");

            }
            reader.NextResult();

            while(reader.Read())
            {
                recipient.DateOfBirth = reader.GetDateTime("DateOfBirth");
                recipient.KnownAs = reader.GetString("KnownAs");
                recipient.Created = reader.GetDateTime("Created");
                recipient.LastActive = reader.GetDateTime("LastActive");
                recipient.Gender = reader.GetString("Gender");
                recipient.Introduction = (reader.IsDBNull("Introduction")) ? null : reader.GetString("Introduction");
                recipient.LookingFor = (reader.IsDBNull("LookingFor")) ? null : reader.GetString("LookingFor");
                recipient.Interests = (reader.IsDBNull("Interests")) ? null : reader.GetString("Interests");
                recipient.City = reader.GetString("City");
                recipient.Country = reader.GetString("Country");

                return new Message
                {
                    Id = reader.GetInt32("Id"),
                    SenderId = reader.GetInt32("SenderId"),
                    SenderUsername = reader.GetString("SenderUsername"),
                    Sender = sender,
                    RecipientId = reader.GetInt32("RecipientId"),
                    RecipientUsername = reader.GetString("RecipientUsername"),
                    Recipient = recipient,
                    Content = reader.GetString("Content"),
                    DateRead = (reader.IsDBNull("DateRead")) ? null : reader.GetDateTime("DateRead"),
                    MessageSent = reader.GetDateTime("MessageSent"),
                    SenderDeleted = reader.GetBoolean("SenderDeleted"),
                    RecipientDeleted = reader.GetBoolean("RecipientDeleted")
                };
                
            }

            return null;
            // return await _context.Messages
            //     .Include(u => u.Sender)
            //     .Include(u => u.Recipient)
            //     .SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Group> GetMessageGroup(string groupName) //DONE
        {
            Console.WriteLine("GET MESSAGE GROUP");
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.GetMessageGroup";
            command.CommandType = CommandType.StoredProcedure;  
            command.Parameters.AddWithValue("@groupName", groupName);

            var connections = new List<Connection>();

            using var reader = await command.ExecuteReaderAsync();
            while(reader.Read())
            {
                var nullCheck = (reader.IsDBNull("ConnectionId")) ? null : reader.GetString("ConnectionId");
                
                if (nullCheck == null) return null;
                
                connections.Add(new Connection
                {
                    ConnectionId = reader.GetString("ConnectionId"),
                    Username = reader.GetString("Username")
                });

                return new Group()
                {
                    Name = reader.GetString("GroupName"),
                    Connections = connections
                };
            }

            return null;

            // return await _context.Groups
            //     .Include(x => x.Connections)
            //     .FirstOrDefaultAsync(x => x.Name == groupName);
         
        }

        public PagedList<MessageDTO> GetMessagesForUser(MessageParams messageParams) 
        {
            Console.WriteLine("GET MESSAGES FOR USER");

            var messages = new List<MessageDTO>();

            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.GetMessagesForUser";
            command.CommandType = CommandType.StoredProcedure;

            if (messageParams.Container == "Inbox")
            {
                command.Parameters.AddWithValue("@Username", messageParams.Username);
                command.Parameters.AddWithValue("@Container", messageParams.Container);
            }
            if (messageParams.Container == "Outbox")
            {
                command.Parameters.AddWithValue("@Username", messageParams.Username);
                command.Parameters.AddWithValue("@Container", messageParams.Container);
            }
            if (messageParams.Container == "Unread")
            {
                command.Parameters.AddWithValue("@Username", messageParams.Username);
                command.Parameters.AddWithValue("@Container", messageParams.Container);
            }

            using var reader = command.ExecuteReader();

            while(reader.Read())
            {
                messages.Add(new MessageDTO
                {
                    Id = reader.GetInt32("Id"), 
                    SenderId = reader.GetInt32("SenderId"),
                    SenderUsername = reader.GetString("SenderUsername"),
                    SenderPhotoUrl = reader.GetString("SenderPhotoUrl"),
                    RecipientId = reader.GetInt32("RecipientId"),
                    RecipientUsername = reader.GetString("RecipientUsername"),
                    RecipientPhotoUrl = reader.GetString("RecipientPhotoUrl"),
                    Content = reader.GetString("Content"),
                    DateRead = (reader.IsDBNull("DateRead")) ? null : reader.GetDateTime("DateRead"),
                    MessageSent = reader.GetDateTime("MessageSent"),
                    SenderDeleted = reader.GetBoolean("SenderDeleted"),
                    RecipientDeleted = reader.GetBoolean("RecipientDeleted")
                });
            }

            return new PagedList<MessageDTO>(messages, messages.Count(),
                    messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUsername, 
            string recipientUsername) //DONE
        {
            Console.WriteLine("GET MESSAGE THREAD");

            var messages= new List<MessageDTO>();
            int counter = 0;

            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.GetMessageThread";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@currentUsername", currentUsername);
            command.Parameters.AddWithValue("@recipientUsername", recipientUsername);

            using var reader = await command.ExecuteReaderAsync();
            while(reader.Read())
            {
                messages.Add(new MessageDTO
                {
                    Id = reader.GetInt32("Id"),
                    SenderId = reader.GetInt32("SenderId"),
                    SenderUsername = reader.GetString("SenderUsername"),
                    SenderPhotoUrl = reader.GetString("SenderPhotoUrl"),
                    RecipientId = reader.GetInt32("RecipientId"),
                    RecipientUsername = reader.GetString("RecipientUsername"),
                    RecipientPhotoUrl = reader.GetString("RecipientPhotoUrl"),
                    Content = reader.GetString("Content"),
                    DateRead = (reader.IsDBNull("DateRead")) ? null : reader.GetDateTime("DateRead"),
                    MessageSent = reader.GetDateTime("MessageSent"),
                    SenderDeleted = reader.GetBoolean("SenderDeleted"),
                    RecipientDeleted = reader.GetBoolean("RecipientDeleted")
                });

                if(messages[counter].DateRead ==null && 
                messages[counter].RecipientUsername == currentUsername)
                {
                    var readUpdate = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
                    readUpdate.Open();
                    Console.WriteLine(String.Format("UPDATE dbo.Messages SET DateRead = {0} WHERE Id = {1}", DateTime.UtcNow, messages[counter].Id));
                    var sqlCommandUPDATE = String.Format("UPDATE dbo.Messages SET DateRead = SYSDATETIME() WHERE Id = {0}", messages[counter].Id);
                    var CommandUpdate = new SqlCommand(sqlCommandUPDATE, readUpdate);
                    CommandUpdate.ExecuteReader();
                    messages[counter].DateRead = DateTime.UtcNow;
                }

                counter++;
            }

            return messages;

            }

         
        



        public void RemoveConnection(Connection userconnection) //DONE
        {
            Console.WriteLine("Remove Connection");
            Console.WriteLine(userconnection.ConnectionId);
            Console.WriteLine(userconnection.Username);

            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.DeleteConnection";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@connectionId", userconnection.ConnectionId);

            command.ExecuteNonQuery();

        //  _context.Connections.Remove(userconnection);

        }
    }
}

// using System;
// using System.Collections.Generic;
// using System.Data;
// using System.Linq;
// using System.Threading.Tasks;
// using API.DTOs;
// using API.Entities;
// using API.Helpers;
// using API.Interfaces;
// using AutoMapper;
// using AutoMapper.QueryableExtensions;
// using Microsoft.Data.SqlClient;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Configuration;

// namespace API.Data
// {
//     public class MessageRepository : IMessageRepository
//     {
//         private readonly DataContext _context;
//         private readonly IMapper _mapper;
//         private readonly IConfiguration _configuration;
//         public MessageRepository(IConfiguration configuration, DataContext context, IMapper mapper)
//         {
//             _mapper = mapper;
//             _context = context;
//             _configuration = configuration;
//         }

//         public void AddGroup(Group group)
//         {
//             DataTable ConnectionStuff = new DataTable();
//             ConnectionStuff.Columns.Add(new DataColumn("ConnectionId"));
//             ConnectionStuff.Columns.Add(new DataColumn("GroupName"));
//             ConnectionStuff.Columns.Add(new DataColumn("Username"));


//             _context.Groups.Add(group);
//         }

//         public void AddMessage(Message message)
//         {
//            _context.Messages.Add(message);
//         }

//         public void DeleteMessage(Message message)
//         {
//             _context.Messages.Remove(message);
//         }

//         public async Task<Connection> GetConnection(string connectionId)
//         {
//             return await _context.Connections.FindAsync(connectionId);
//         }

//         public async Task<Group> GetGroupForConnection(string connectionId)
//         {
//             return await _context.Groups
//                 .Include(c => c.Connections)
//                 .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
//                 .FirstOrDefaultAsync();
//         }

//         public async Task<Message> GetMessage(int id)
//         {
//             return await _context.Messages
//                 .Include(u => u.Sender)
//                 .Include(u => u.Recipient)
//                 .SingleOrDefaultAsync(x => x.Id == id);
//         }

//         public async Task<Group> GetMessageGroup(string groupName)
//         {
//             return await _context.Groups
//                 .Include(x => x.Connections)
//                 .FirstOrDefaultAsync(x => x.Name == groupName);
//         }

//         public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams)
//         {
//             var query = _context.Messages
//                 .OrderByDescending(m => m.MessageSent)
//                 .ProjectTo<MessageDTO>(_mapper.ConfigurationProvider)
//                 .AsQueryable();

//             query = messageParams.Container switch
//             {
//                 "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username 
//                     && u.RecipientDeleted == false),
//                 "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username
//                     && u.SenderDeleted == false),
//                 _ => query.Where(u => u.RecipientUsername ==
//                     messageParams.Username && u.RecipientDeleted == false && u.DateRead == null)
//             };

//             return await PagedList<MessageDTO>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);

//         }
//            public void AddConnection(Connection connectionObject, string groupName)
//         {

//             using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
//             connection.Open();
//             using var command = connection.CreateCommand();
//             command.CommandText = "dbo.AddConnection";
//             command.CommandType = CommandType.StoredProcedure;
//             command.Parameters.AddWithValue("@ConnectionId", connectionObject.ConnectionId);
//             command.Parameters.AddWithValue("@Username", connectionObject.Username);
//             command.Parameters.AddWithValue("@GroupName", groupName);

//             command.ExecuteNonQuery();
//         }

//         public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUsername, 
//             string recipientUsername)
//         {
//             var messages = await _context.Messages
//                 .Where(m => m.Recipient.UserName == currentUsername && m.RecipientDeleted == false
//                         && m.Sender.UserName == recipientUsername
//                         || m.Recipient.UserName == recipientUsername
//                         && m.Sender.UserName == currentUsername && m.SenderDeleted == false
//                 )
//                 .OrderBy(m => m.MessageSent)
//                 .ProjectTo<MessageDTO>(_mapper.ConfigurationProvider)
//                 .ToListAsync();

//             var unreadMessages = messages.Where(m => m.DateRead == null 
//                 && m.RecipientUsername == currentUsername).ToList();

//             if (unreadMessages.Any())
//             {
//                 foreach (var message in unreadMessages)
//                 {
//                     message.DateRead = DateTime.UtcNow;
//                 }
//             }

//             return messages;
//         }

//         public void RemoveConnection(Connection connection)
//         {
//             _context.Connections.Remove(connection);
//         }
//     }
// }