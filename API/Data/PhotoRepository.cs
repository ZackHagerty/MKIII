using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly IConfiguration _configuration;

        public PhotoRepository(DataContext context, IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        }


        public void AddPhoto(Photo photo)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command =  connection.CreateCommand();
            command.CommandText = "dbo.AddPhoto";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Url", photo.Url);
            command.Parameters.AddWithValue("@IsMain", photo.IsMain);
            command.Parameters.AddWithValue("@PublicId", photo.PublicId);
            command.Parameters.AddWithValue("@AppUserId", photo.AppUserId);
            command.Parameters.AddWithValue("@isApproved", photo.IsApproved);

            command.ExecuteNonQuery();
        }

        public Photo GetPhotoById(int id)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.GetPhotoById";
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                return new Photo()
                {
                    Id = reader.GetInt32("Id"),
                    Url = reader.GetString("Url"),
                    IsMain = reader.GetBoolean("IsMain"),
                    IsApproved = reader.GetBoolean("IsApproved"),
                    PublicId = (reader.IsDBNull("PublicId")) ? null : reader.GetString("PublicId"),
                    AppUserId = reader.GetInt32("AppUserId")
                };
            }

            return null;
        }

        public async Task<IEnumerable<PhotoForApprovalDTO>> GetUnapprovedPhotos()
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.GetUnapprovedPhotos";
            command.CommandType = System.Data.CommandType.StoredProcedure;
            
            var result = new List<PhotoForApprovalDTO>();

            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                result.Add(new PhotoForApprovalDTO
                {
                    IsApproved = reader.GetBoolean("IsApproved"),
                    Id = reader.GetInt32("Id"),
                    Url = reader.GetString("Url"),
                    Username = reader.GetString("Username")
                });
            }

            return result;
        }

        public async void RemovePhoto(Photo photo)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "dbo.DeletePhoto";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", photo.Id);

            await command.ExecuteNonQueryAsync();


        }
    }
}