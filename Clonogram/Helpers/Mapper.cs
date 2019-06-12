using System;
using AutoMapper;
using Clonogram.Models;
using Clonogram.ViewModels;
using NpgsqlTypes;

namespace Clonogram.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserView>()
                .ForMember("Id", x => x.MapFrom(y => y.Id.ToString()));
            CreateMap<UserView, User>()
                .ForMember("Id", x => x.MapFrom(y => Guid.Parse(y.Id)));

            CreateMap<Photo, PhotoView>()
                .ForMember("Id", x => x.MapFrom(y => y.Id.ToString()))
                .ForMember("UserId", x => x.MapFrom(y => y.UserId.ToString()))
                .ForMember("Longitude", x => x.MapFrom(y => y.Geo.X))
                .ForMember("Latitude", x => x.MapFrom(y => y.Geo.Y));
            CreateMap<PhotoView, Photo>()
                .ForMember("Id", x => x.MapFrom(y => Guid.Parse(y.Id)))
                .ForMember("UserId", x => x.MapFrom(y => Guid.Parse(y.UserId)))
                .ForMember("Geo", x => x.MapFrom(y => new NpgsqlPoint(y.Longitude, y.Latitude)));
        }
    }
}