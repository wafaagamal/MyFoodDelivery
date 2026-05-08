using AutoMapper;
using DeliverySvc.Application.Contracts.DeliveryTasks;
using DeliverySvc.Application.Contracts.Riders;
using DeliverySvc.Domain.DeliveryTasks;
using DeliverySvc.Domain.Riders;

namespace DeliverySvc.Application;

public class DeliverySvcAutoMapperProfile : Profile
{
    public DeliverySvcAutoMapperProfile()
    {
        CreateMap<Rider, RiderDto>();
        CreateMap<Rider, RiderListDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName));
        CreateMap<DeliveryTask, DeliveryTaskDto>();
    }
}
