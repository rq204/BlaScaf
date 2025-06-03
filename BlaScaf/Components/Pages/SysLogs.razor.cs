using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AntDesign;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlaScaf.Components.Pages
{
    public partial class SysLogs
    {
        [Inject] MessageService MessageService { get; set; }

        AntDesign.ITable table;

        private bool isloading = false;

        private string keyword = null;
        private string lastKeyword = null;

        protected override async Task OnInitializedAsync()
        {
            await this.PageIndexSizeChange();
            await base.OnInitializedAsync();
        }


        private QueryRsp<List<BsSysLog>> logsrsp = new QueryRsp<List<BsSysLog>>();

        private async void PageIndexChange(int pageIndex)
        {
            this.pageIndex = pageIndex;
            await PageIndexSizeChange();
        }

        private async void PageSizeChange(int pageSize)
        {
            this.pageSize = pageSize;
            await PageIndexSizeChange();
        }

        private int optUserId = 0;
        private int pageIndex = 0;
        private int pageSize = 15;
        private async Task PageIndexSizeChange()
        {
            isloading = true;
            this.logsrsp = BsConfig.GetSysLogs(this.pageIndex, pageSize);
            isloading = false;
            await base.InvokeAsync(base.StateHasChanged);
        }
    }
}