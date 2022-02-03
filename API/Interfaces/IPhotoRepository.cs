using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;

namespace API.Interfaces
{
    public interface IPhotoRepository
    {
        Task<IEnumerable<PhotoForApprovalDTO>> GetUnapprovedPhotos();

        Photo GetPhotoById(int id);

        void RemovePhoto(Photo photo);

        public void AddPhoto(Photo photo);
    }
}