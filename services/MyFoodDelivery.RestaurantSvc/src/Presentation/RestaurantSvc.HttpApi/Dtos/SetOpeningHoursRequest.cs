using System.Collections.Generic;

public record SetOpeningHoursRequest(List<OpeningHoursItem> Hours);
public record OpeningHoursItem(int Day, string OpenTime, string CloseTime, bool IsClosed);
