using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SafePath.DTOs;
using SafePath.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;

namespace SafePath.Blazor.Pages.Admin;

public partial class Index
{
    public IAreaService AreaService { get; set; }

    protected IList<AreaDto>? Areas { get; set; }

    protected AreaDto? SelectedArea { get; set; }

    protected GeoJsonFeatureCollection SecurityElements { get; set; }


    protected override async Task OnInitializedAsync()
    {
        var areas = await AreaService.GetAdminAreas();

        if (areas.Count == 0) //in theory it will never happen
            throw new AbpException("User is not allowed to access the Area admin page");

        // Check if the user is a normal user or system admin.
        if (areas.Count > 1)
        {
            Areas = areas; //user is sys admin
        }

        SelectedArea = areas[0];

        await JSRuntime.InvokeVoidAsync("waitForMapLibre", SelectedArea.InitialLatitude, SelectedArea.InitialLongitude, SecurityElements);
    }

    private async Task OnAreaSelected(ChangeEventArgs e)
    {
        var selectedAreaId = e.Value!.ToString();
        SelectedArea = Areas!.First(area => area.Id.ToString() == selectedAreaId);

        // Here, you can update the map with new coordinates based on the selected Area.
        await JSRuntime.InvokeVoidAsync("updateMapCoordinates", SelectedArea.InitialLatitude, SelectedArea.InitialLongitude);
    }


    private async Task UpdateMapData()
    {
        // Add logic to update map data
    }

    private async Task UpdateCrimeRate()
    {
        // Add logic to update crime rate
    }

    private async Task ToggleLayer(string layerType)
    {
        // If we haven't already fetched the SecurityElements, fetch them
        bool sendSecurityElements = false;
        if (SecurityElements == null)
        {
            await JSRuntime.InvokeVoidAsync("showLoadingOverlay");
            SecurityElements = await AreaService.GetSecurityLayerGeoJSON();
            sendSecurityElements = true;
            await JSRuntime.InvokeVoidAsync("hideLoadingOverlay");
        }

        // Send the data to JavaScript to either create or toggle the visibility of the layer
        await JSRuntime.InvokeVoidAsync("showElements", layerType, sendSecurityElements ? SecurityElements : null);
    }


    public class MapBounds
    {
        public Coordinate northeast { get; set; }
        public Coordinate southwest { get; set; }
    }

    public class Coordinate
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }
}
