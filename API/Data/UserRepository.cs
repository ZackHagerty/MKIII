using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;

        public UserRepository(IConfiguration configuration, DataContext context, IMapper mapper)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _context = context;
        }

        public List<PhotoDTO> GetPhotoDTO(string username) //DONE
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.GetMembersPhotos";
            command.CommandType = CommandType.StoredProcedure;  
            command.Parameters.AddWithValue("@username", username);

            var photos = new List<PhotoDTO>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                photos.Add(new PhotoDTO()
                {
                    Id = reader.GetInt32("Id"),
                    Url = reader.GetString("Url"),
                    IsMain = reader.GetBoolean("IsMain")
                });
            }

            reader.Close();
            return photos;

        }
        public async Task<MemberDTO> GetMemberAsync(string username, bool? isCurrentUser) //DONE
        {
         var photos = GetPhotoDTO(username);
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.GetMemberAsync";
            command.CommandType = CommandType.StoredProcedure;  
            command.Parameters.AddWithValue("@username", username);


            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                return new MemberDTO
                {
                    Id = reader.GetInt32("Id"),
                    Username = reader.GetString("UserName"),
                    PhotoUrl = reader.GetString("Url"),
                    Age = reader.GetDateTime("DateOfBirth").CalculateAge(),
                    KnownAs = reader.GetString("KnownAs"),
                    Created = reader.GetDateTime("Created"),
                    LastActive = reader.GetDateTime("LastActive"),
                    Gender = reader.GetString("Gender"),
                    Introduction = (reader.IsDBNull("Introduction")) ? null : reader.GetString("Introduction"),
                    LookingFor = (reader.IsDBNull("LookingFor")) ? null : reader.GetString("LookingFor"),
                    Interests = (reader.IsDBNull("Interests")) ? null : reader.GetString("Interests"),
                    City = reader.GetString("City"),
                    Country = reader.GetString("Country"),
                    Photos = (ICollection<PhotoDTO>)photos
                };
            }

            return null;
        }

        public PagedList<MemberDTO> GetMembersAsync(UserParams userParams)
        {
            var photos = GetPhotoDTO(userParams.CurrentUsername);
            var members = new List<MemberDTO>();

            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.GetMembersAsync";
            command.CommandType = CommandType.StoredProcedure;

            if (userParams.OrderBy == "created")
            {
                command.Parameters.AddWithValue("@Username", userParams.CurrentUsername);
                command.Parameters.AddWithValue("@Gender", userParams.Gender);
                command.Parameters.AddWithValue("@minDob", minDob);
                command.Parameters.AddWithValue("@maxDob", maxDob);
                command.Parameters.AddWithValue("@OrderBy", userParams.OrderBy);

            }
            if (userParams.OrderBy == "lastActive")
            {
                command.Parameters.AddWithValue("@Username", userParams.CurrentUsername);
                command.Parameters.AddWithValue("@Gender", userParams.Gender);
                command.Parameters.AddWithValue("@minDob", minDob);
                command.Parameters.AddWithValue("@maxDob", maxDob);
                command.Parameters.AddWithValue("@OrderBy", userParams.OrderBy);
            }

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                members.Add(new MemberDTO
                {
                    Id = reader.GetInt32("Id"),
                    Username = reader.GetString("Username"),
                    PhotoUrl = reader.GetString("Url"),
                    Age = reader.GetDateTime("DateOfBirth").CalculateAge(),
                    KnownAs = reader.GetString("KnownAs"),
                    Created = reader.GetDateTime("Created"),
                    LastActive = reader.GetDateTime("LastActive"),
                    Gender = reader.GetString("Gender"),
                    Introduction = (reader.IsDBNull("Introduction")) ? null : reader.GetString("Introduction"),
                    LookingFor = (reader.IsDBNull("LookingFor")) ? null : reader.GetString("LookingFor"),
                    Interests = (reader.IsDBNull("Interests")) ? null : reader.GetString("Interests"),
                    City = reader.GetString("City"),
                    Country = reader.GetString("Country"),
                    Photos = (ICollection<PhotoDTO>)photos
                });
            }

            
            return new PagedList<MemberDTO>(members, members.Count(), 
                    userParams.PageNumber, userParams.PageSize);
          
        }

        public async Task<AppUser> GetUserByIdAsync(int id) //DONE
        {
            Console.WriteLine(id);
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.GetUserByIdAsync";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@id", id);

            var appUser = new AppUser();
            var photos = new List<Photo>();
            var likedByUsers = new List<UserLike>();
            var likedUsers = new List<UserLike>();
            var messagesReceived = new List<Message>();
            var messagesSent = new List<Message>();
            var userRoles = new List<AppUserRole>();

                appUser.UserRoles = userRoles.Where(r => r.UserId == appUser.Id).ToList();
                foreach(var role in appUser.UserRoles)
                    role.User = appUser;
            

            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                appUser.Id = reader.GetInt32("Id");
                appUser.DateOfBirth = reader.GetDateTime("DateOfBirth");
                appUser.UserName = reader.GetString("UserName");
                appUser.KnownAs = (reader.IsDBNull("KnownAs")) ? null : reader.GetString("KnownAs");
                appUser.Created = reader.GetDateTime("Created");
                appUser.LastActive = reader.GetDateTime("LastActive");
                appUser.Gender = (reader.IsDBNull("Gender")) ? null : reader.GetString("Gender");
                appUser.Introduction = (reader.IsDBNull("Introduction")) ? null : reader.GetString("Introduction");
                appUser.LookingFor = (reader.IsDBNull("LookingFor")) ? null : reader.GetString("LookingFor");
                appUser.Interests = (reader.IsDBNull("Interests")) ? null : reader.GetString("Interests");
                appUser.City = (reader.IsDBNull("City")) ? null : reader.GetString("City");
                appUser.Country = (reader.IsDBNull("Country")) ? null : reader.GetString("Country");
            }
            reader.NextResult();
            while (reader.Read())
            {
                photos.Add(
                    new Photo
                    {
                        Id = reader.GetInt32("Id"),
                        Url = reader.GetString("Url"),
                        IsMain = reader.GetBoolean("IsMain"),
                        AppUserId = reader.GetInt32("AppUserId")
                    });
            }
            reader.NextResult();
            while (reader.Read())
            {
                likedByUsers.Add(
                    new UserLike
                    {
                        SourceUserId = reader.GetInt32("SourceUserId"),
                        LikedUserId = reader.GetInt32("LikedUserId")
                    });
            }
            reader.NextResult();
            while (reader.Read())
            {
                likedUsers.Add(
                    new UserLike
                    {
                        SourceUserId = reader.GetInt32("SourceUserId"),
                        LikedUserId = reader.GetInt32("LikedUserId")
                    });
            }
            reader.NextResult();
            while (reader.Read())
            {

                messagesReceived.Add(
                    new Message
                    {
                        Id = reader.GetInt32("Id"),
                        SenderId = reader.GetInt32("SenderId"),
                        SenderUsername = reader.GetString("SenderUsername"),
                        RecipientId = reader.GetInt32("RecipientId"),
                        RecipientUsername = reader.GetString("RecipientUsername"),
                        Content = reader.GetString("Content"),
                        MessageSent = reader.GetDateTime("MessageSent"),
                        SenderDeleted = reader.GetBoolean("SenderDeleted"),
                        RecipientDeleted = reader.GetBoolean("RecipientDeleted"),
                        DateRead = (reader.IsDBNull("DateRead")) ? null : reader.GetDateTime("DateRead")
                    });
            }
            reader.NextResult();
            while (reader.Read())
            {
                messagesSent.Add(
                new Message
                {
                    Id = reader.GetInt32("Id"),
                    SenderId = reader.GetInt32("SenderId"),
                    SenderUsername = reader.GetString("SenderUsername"),
                    RecipientId = reader.GetInt32("RecipientId"),
                    RecipientUsername = reader.GetString("RecipientUsername"),
                    Content = reader.GetString("Content"),
                    DateRead = (reader.IsDBNull("DateRead")) ? null : reader.GetDateTime("DateRead"),
                    MessageSent = reader.GetDateTime("MessageSent"),
                    SenderDeleted = reader.GetBoolean("SenderDeleted"),
                    RecipientDeleted = reader.GetBoolean("RecipientDeleted")
                });
            }
            appUser.Photos = photos;
            appUser.LikedByUsers = likedByUsers;
            appUser.LikedUsers = likedUsers;
            appUser.MessagesReceived = messagesReceived;
            appUser.MessagesSent = messagesSent;
        
            if (appUser != null) return appUser;
            return null;
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.GetUserByUsernameAsync";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@username", username);

            var appUser = new AppUser();
            var photos = new List<Photo>();
            var likedByUsers = new List<UserLike>();
            var likedUsers = new List<UserLike>();
            var messagesReceived = new List<Message>();
            var messagesSent = new List<Message>();
            var userRoles = new List<AppUserRole>();

            appUser.UserRoles = userRoles.Where(r => r.UserId == appUser.Id).ToList();
            foreach(var role in appUser.UserRoles)
            role.User = appUser;

            
            

            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                appUser.Id = reader.GetInt32("Id");
                appUser.DateOfBirth = reader.GetDateTime("DateOfBirth");
                appUser.UserName = reader.GetString("UserName");
                appUser.KnownAs = reader.GetString("KnownAs");
                appUser.Created = reader.GetDateTime("Created");
                appUser.LastActive = reader.GetDateTime("LastActive");
                appUser.Gender = reader.GetString("Gender");
                appUser.Introduction = (reader.IsDBNull("Introduction")) ? null : reader.GetString("Introduction");
                appUser.LookingFor = (reader.IsDBNull("LookingFor")) ? null : reader.GetString("LookingFor");
                appUser.Interests = (reader.IsDBNull("Interests")) ? null : reader.GetString("Interests");
                appUser.City = reader.GetString("City");
                appUser.Country = reader.GetString("Country");
            }
            reader.NextResult();
            while (reader.Read())
            {
                photos.Add(
                    new Photo
                    {
                        Id = reader.GetInt32("Id"),
                        Url = reader.GetString("Url"),
                        IsMain = reader.GetBoolean("IsMain"),
                        AppUserId = reader.GetInt32("AppUserId")
                    });
            }
            reader.NextResult();
            while (reader.Read())
            {
                likedByUsers.Add(
                    new UserLike
                    {
                        SourceUserId = reader.GetInt32("SourceUserId"),
                        LikedUserId = reader.GetInt32("LikedUserId")
                    });
            }
            reader.NextResult();
            while (reader.Read())
            {
                likedUsers.Add(
                    new UserLike
                    {
                        SourceUserId = reader.GetInt32("SourceUserId"),
                        LikedUserId = reader.GetInt32("LikedUserId")
                    });
            }
            reader.NextResult();
            while (reader.Read())
            {

                messagesReceived.Add(
                    new Message
                    {
                        Id = reader.GetInt32("Id"),
                        SenderId = reader.GetInt32("SenderId"),
                        SenderUsername = reader.GetString("SenderUsername"),
                        RecipientId = reader.GetInt32("RecipientId"),
                        RecipientUsername = reader.GetString("RecipientUsername"),
                        Content = reader.GetString("Content"),
                        MessageSent = reader.GetDateTime("MessageSent"),
                        SenderDeleted = reader.GetBoolean("SenderDeleted"),
                        RecipientDeleted = reader.GetBoolean("RecipientDeleted"),
                        DateRead = (reader.IsDBNull("DateRead")) ? null : reader.GetDateTime("DateRead")
                    });
            }
            reader.NextResult();
            while (reader.Read())
            {
                messagesSent.Add(
                new Message
                {
                    Id = reader.GetInt32("Id"),
                    SenderId = reader.GetInt32("SenderId"),
                    SenderUsername = reader.GetString("SenderUsername"),
                    RecipientId = reader.GetInt32("RecipientId"),
                    RecipientUsername = reader.GetString("RecipientUsername"),
                    Content = reader.GetString("Content"),
                    DateRead = (reader.IsDBNull("DateRead")) ? null : reader.GetDateTime("DateRead"),
                    MessageSent = reader.GetDateTime("MessageSent"),
                    SenderDeleted = reader.GetBoolean("SenderDeleted"),
                    RecipientDeleted = reader.GetBoolean("RecipientDeleted")
                });
            }
            appUser.Photos = photos;
            appUser.LikedByUsers = likedByUsers;
            appUser.LikedUsers = likedUsers;
            appUser.MessagesReceived = messagesReceived;
            appUser.MessagesSent = messagesSent;
        
            if (appUser != null) return appUser;
            return null;
        }

        public async Task<string> GetUserGender(string username) //DONE
        { 
            string Gender = null;

            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.GetUserGender";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@username", username);

            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                Gender = (reader.IsDBNull("Gender")) ? null : reader.GetString("Gender");
            }
            
            return Gender;
        }
        

        public async Task<IEnumerable<AppUser>> GetUsersAsync() //No references, DONE
        {
            // var users = new List<AppUser>();
            // //populate users with reader.Read()

            // var userRoles = new List<AppUserRole>();
            // //populate dict from reader.Read()
            // foreach (var user in users)
            // {
            //     user.UserRoles = userRoles.Where(r => r.UserId == user.Id).ToList();
            //     foreach(var role in user.UserRoles)
            //         role.User = user;
            // }

            //  return await _context.Users
            //      .Include(p => p.Photos)
            //     .ToListAsync();
            return null;
        }

        public void Update(AppUser user) //DONE
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.UpdateUser";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@userId", user.Id);
            command.Parameters.AddWithValue("@Introduction", user.Introduction);
            command.Parameters.AddWithValue("@LookingFor", user.LookingFor);
            command.Parameters.AddWithValue("@City", user.City);
            command.Parameters.AddWithValue("@Country", user.Country);

           command.ExecuteNonQuery();
        }

        public async Task<AppUser> GetUserByPhotoId(int photoId)
        {
            Console.WriteLine("GETTING USER BY PHOTO ID");
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.GetUserByPhotoId";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@photoId", photoId);

            var appUser = new AppUser();
            var PhotosList = new List<Photo>();

            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                appUser.Id = reader.GetInt32("Id");
                appUser.DateOfBirth = reader.GetDateTime("DateOfBirth");
                appUser.KnownAs = reader.GetString("KnownAs");
                appUser.Created = reader.GetDateTime("Created");
                appUser.LastActive = reader.GetDateTime("LastActive");
                appUser.Gender = reader.GetString("Gender");
                appUser.Introduction = (reader.IsDBNull("Introduction")) ? null : reader.GetString("Introduction");
                appUser.LookingFor = (reader.IsDBNull("LookingFor")) ? null : reader.GetString("LookingFor");
                appUser.Interests = (reader.IsDBNull("Interests")) ? null : reader.GetString("Interests");
                appUser.City = reader.GetString("City");
                appUser.Country = reader.GetString("Country");      
                
                PhotosList.Add(new Photo
                {
                Id = reader.GetInt32("photoId"),
                Url = reader.GetString("Url"),
                IsApproved = reader.GetBoolean("IsApproved"),
                PublicId = null,
                AppUser = appUser,
                AppUserId = reader.GetInt32("AppUserId")
                });

            
            }
            appUser.Photos = PhotosList;

            return appUser;
        }
    }
}