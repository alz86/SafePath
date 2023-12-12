using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using SafePath.DTOs;
using SafePath.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Components.Messages;

namespace SafePath.Blazor.Pages.Admin;

public partial class Index
{
    private readonly IUiMessageService uiMessageService;

    /// <summary>
    /// Service to access the Area API. 
    /// </summary>
    private readonly IAreaService areaService;

    /// <summary>
    /// Service to access services related to
    /// Area data. 
    /// </summary>
    private readonly IAreaDataService areaDataService;
    private readonly IClientDataValidator clientDataValidator;

    private IBrowserFile? selectedFile;
    private bool mapLibreInitCalled = false;

    public Index(IUiMessageService uiMessageService, IAreaService areaService, IAreaDataService areaDataService, IClientDataValidator clientDataValidator)
    {
        this.uiMessageService = uiMessageService;
        this.areaService = areaService;
        this.areaDataService = areaDataService;
        this.clientDataValidator = clientDataValidator;
    }


    /// <summary>
    /// List of areas the current user can administrate.
    /// </summary>
    /// <remarks>
    /// The system currently supports only one area, but in a
    /// next phase will support many.
    /// </remarks>
    protected IList<AreaDto>? Areas { get; set; }

    /// <summary>
    /// Area selected by the user.
    /// </summary>
    protected AreaDto? SelectedArea { get; set; }

    protected GeoJsonFeatureCollection? SecurityElements { get; set; }

    protected bool ShowLoading { get; set; }

    protected bool ShowCrimeDataDialog { get; set; }

    protected bool ShowBulkDataDialog { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var areas = await areaService.GetAdminAreas();

        if (areas.Count == 0) //in theory it will never happen
            throw new AbpException("User is not allowed to access the Area admin page");

        // Check if the user is a normal user or system admin.
        if (areas.Count > 1)
        {
            Areas = areas; //user is sys admin
        }

        SelectedArea = areas[0];

        await JSRuntime.InvokeAsync<IJSObjectReference>("import", "/maplibre/maplibre-gl-dev-3.6.2.js");
        await JSRuntime.InvokeAsync<IJSObjectReference>("import", "/Pages/Admin/Index.razor.js");
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (!mapLibreInitCalled && SelectedArea != null)
        {
            await JSRuntime.InvokeVoidAsync("waitForMapLibre", SelectedArea!.InitialLatitude, SelectedArea.InitialLongitude, SecurityElements);
            mapLibreInitCalled = true;
        }
    }

    private async Task OnAreaSelected(ChangeEventArgs e)
    {
        var selectedAreaId = e.Value!.ToString();

        SelectedArea = Areas!.First(area => area.Id.ToString() == selectedAreaId);

        // Here, you can update the map with new coordinates based on the selected Area.
        await JSRuntime.InvokeVoidAsync("updateMapCoordinates", SelectedArea.InitialLatitude, SelectedArea.InitialLongitude);
    }

    private async Task ToggleLayer(string layerType)
    {
        // If we haven't already fetched the SecurityElements, fetch them
        bool securityElementsSet = false;
        if (SecurityElements == null)
        {
            ShowLoading = true;
            try
            {
                SecurityElements = await areaService.GetSecurityLayerGeoJSON(SelectedArea!.Id);
                securityElementsSet = true;
            }
            catch (Exception ex)
            {
                await uiMessageService.Error(ex.Message);
            }
            finally
            {
                ShowLoading = false;
            }
        }

        // Send the data to JavaScript to either create or toggle the visibility of the layer
        await JSRuntime.InvokeVoidAsync("showElements", layerType, securityElementsSet ? SecurityElements : null);
    }

    private void OnFileSelected(InputFileChangeEventArgs e)
    {
        selectedFile = e.File;
    }

    private async Task<string> ReadFileContent()
    {
        if (selectedFile == null)
            throw new UserFriendlyException("There is not file selected to upload.");
        else if (selectedFile.Size == 0)
            throw new UserFriendlyException("The selected file is empty.");
        else if (selectedFile.Size > Constants.MaxCsvFileSize)
            throw new UserFriendlyException("The selected file to upload is too big. The maximum size allowed is 50MB.");

        using (var stream = new StreamReader(selectedFile.OpenReadStream()))
            return await stream.ReadToEndAsync();

    }

    private async Task UploadCSVFile(bool isCrimeData)
    {
        ShowLoading = true;

        try
        {
            var fileContent = await ReadFileContent();
            if (isCrimeData)
            {
                await clientDataValidator.ValidateCrimeReportCSVFile(fileContent);
                await areaDataService.UploadCrimeReportCSV(SelectedArea!.Id, fileContent);

            }
            else
            {
                await areaDataService.UploadCrimeReportCSV(SelectedArea!.Id, fileContent);

            }
            await uiMessageService.Success("The file was uploaded successfully.");
        }
        catch (Exception ex)
        {
            await uiMessageService.Error(ex.Message);
        }
        finally
        {
            ShowLoading = false;
            this.StateHasChanged();
        }
    }

    private async Task HandleCrimeDataClosing(bool isCancelling)
    {
        ShowCrimeDataDialog = false;
        if (!isCancelling) await UploadCSVFile(true);
        selectedFile = null;
    }

    private async Task HandleBulkDataClosing(bool isCancelling)
    {
        ShowBulkDataDialog = false;
        if (!isCancelling) await UploadCSVFile(false);
        selectedFile = null;
    }
}