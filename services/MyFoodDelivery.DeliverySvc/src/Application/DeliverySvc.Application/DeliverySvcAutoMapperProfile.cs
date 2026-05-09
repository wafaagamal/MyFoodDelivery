using AutoMapper;
using DeliverySvc.Application.Contracts.DeliveryTasks;
using DeliverySvc.Application.Contracts.DeliveryTasks.Dtos;
using DeliverySvc.Application.Contracts.Riders;
using DeliverySvc.Application.Contracts.Riders.Dtos;
using DeliverySvc.Domain.DeliveryTasks;
using DeliverySvc.Domain.Riders;

namespace DeliverySvc.Application;

public class DeliverySvcAutoMapperProfile : Profile
{
    public DeliverySvcAutoMapperProfile()
    {
        CreateMap<Rider, RiderDto>();
        CreateMap<Rider, RiderListDto>();
        CreateMap<DeliveryTask, DeliveryTaskDto>();
    }
}
