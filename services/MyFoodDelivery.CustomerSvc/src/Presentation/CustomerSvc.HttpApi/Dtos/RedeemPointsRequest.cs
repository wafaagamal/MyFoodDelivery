using System;

namespace CustomerSvc.HttpApi.Dtos;

public record RedeemPointsRequest(
    int Points,
    Guid? OrderId);
