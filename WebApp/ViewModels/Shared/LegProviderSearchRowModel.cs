using BLL.DTO.Entities;

namespace WebApp.ViewModels.Shared;

public record LegProviderSearchRowModel(LegProviderSummary Summary, string CollapseId, bool IsSubLeg = false);