using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace API.Data
{
    public class OldLikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public OldLikesRepository(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, likedUserId);
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
                    Id = reader.GetInt32("SourceUserId")
                });
            }
            //Don't change this lol
            return new PagedList<LikeDTO>(likedUsers, likedUsers.Count(), 
                likesParams.PageNumber, likesParams.PageSize);   
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users
                .Include(x => x.LikedUsers)
                .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
} 