using AutoMapper;
using WebSocketServer.Entities;
using WebSocketServer.Model;

namespace WebSocketServer.Profiles
{
    public class RunClockProfile : Profile
    {
        public RunClockProfile()
        {
            CreateMap<RunClock, RunClockDto>()
                .ForMember(
                    dest => dest.KeepTime,
                    opt => opt.MapFrom(src => (int)src.EndTime.Subtract(src.StartTime).TotalSeconds));
        }
    }
}