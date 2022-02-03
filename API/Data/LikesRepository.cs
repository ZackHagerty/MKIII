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
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly IConfiguration _configuration;

        private readonly DataContext _context;
        public LikesRepository(IConfiguration configuration, DataContext context)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _context = context;
        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId) //DONE
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.GetUserLike";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", sourceUserId);
            command.Parameters.AddWithValue("@likedId", likedUserId);

            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                return new UserLike()
                {

                    SourceUserId = reader.GetInt32("SourceUserId"),
                    LikedUserId = reader.GetInt32("LikeduserId")

                };
            } 

            return null;
        }

        public PagedList<LikeDTO> GetUserLikes(LikesParams likesParams)
        {
           using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
           connection.Open();
           using var command = connection.CreateCommand();
           command.CommandText = "dbo.GetUserLikes";
           command.CommandType = CommandType.StoredProcedure;

            if (likesParams.Predicate == "liked")
            {
                command.Parameters.AddWithValue("@UserId", likesParams.UserId);
                command.Parameters.AddWithValue("@Predicate", likesParams.Predicate);
            }

            if (likesParams.Predicate == "likedBy")
            {
                command.Parameters.AddWithValue("@UserId", likesParams.UserId);
                command.Parameters.AddWithValue("@Predicate", likesParams.Predicate);
            }

            var likedUsers = new List<LikeDTO>();

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                likedUsers.Add(new LikeDTO
                {
                    Username = reader.GetString("UserName"),
                    KnownAs = reader.GetString("KnownAs"),
                    Age = reader.GetDateTime("DateOfBirth").CalculateAge(),
                    PhotoUrl = reader.GetString("Url"),
                    City = reader.GetString("City"),
                    Id = (likesParams.Predicate == "liked") ? reader.GetInt32("LikedUserId") : reader.GetInt32("SourceUserId")
                });
            }
            //Don't change this lol
            return new PagedList<LikeDTO>(likedUsers, likedUsers.Count(), 
                likesParams.PageNumber, likesParams.PageSize);

           
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            Console.WriteLine("GET USER WITH LIKEs");
            Console.WriteLine(userId);
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.GetUserWithLikes";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@userId", userId);

            var appUser = new AppUser();
            var likedUsers = new List<UserLike>();

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

                // likedUsers.Add( new UserLike
                // {
                //     SourceUserId = reader.GetInt32("SourceUserId"),
                //     LikedUserId = reader.GetInt32("LikedUserId")
                // });
            }
            appUser.LikedUsers = likedUsers;
            // return appUser;
            return await _context.Users
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
} 