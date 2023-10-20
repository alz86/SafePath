using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using SafePath.DTOs;
using SafePath.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Components.Messages;

namespace SafePath.Blazor.Pages.Admin;

public partial class Index
{
    public IUiMessageService UIMessageService { get; set; }

    /// <summary>
    /// Service to access the Area API. 
    /// </summary>
    public IAreaService AreaService { get; set; }

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


    private IBrowserFile? csvFile;

    private void LoadFiles(InputFileChangeEventArgs e)
    {
        csvFile = e.GetMultipleFiles()?.FirstOrDefault();
    }

    private async Task UploadFile()
    {
        if (csvFile == null || csvFile.Size == 0)
            throw new UserFriendlyException("There is not file selected to upload.");
        else if (csvFile.Size == 0)
            throw new UserFriendlyException("The selected file is empty.");
        else if (csvFile.Size > Constants.MaxCsvFileSize)
            throw new UserFriendlyException("The selected file to upload is too big. The maximum size allowed is 50MB.");

        string fileContent;
        using (var stream = new StreamReader(csvFile.OpenReadStream()))
            fileContent = await stream.ReadToEndAsync();

        var resp = await AreaService.UploadCrimeReportCSV(fileContent);
        if (!resp.Success)
        {
            //TODO: add validation errors to the UI
            var errorMessage = resp.ValidationErrors?.Count > 0
                ? "The selected file contains errors. Please fix them and try again."
                : "An error occurred while uploading the file. Please try again later.";
            throw new UserFriendlyException(errorMessage);
        }
        else
        {
            await UIMessageService.Success("The file was uploaded successfully.");
        }
    }
}