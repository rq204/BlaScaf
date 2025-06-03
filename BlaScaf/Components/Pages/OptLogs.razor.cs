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
    public partial class OptLogs
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

        public async void OnSearch(string keyword)
        {
            this.keyword = keyword;
            if (int.TryParse(keyword, out var it))
            {
                if (this.lastKeyword != keyword)
                {
                    this.pageIndex = 1;
                    this.lastKeyword = keyword;
                }
                await this.PageIndexSizeChange();
            }
            else
            {
                if (!string.IsNullOrEmpty(keyword))
                {
                    this.keyword = "";
                   await MessageService.ErrorAsync("任务名必须为数字，请重新输入");
                }
                else
                {
                    if (lastKeyword != keyword)
                    {
                        lastKeyword = null;
                        this.pageIndex = 1;
                    }
                    await this.PageIndexSizeChange();
                }
            }
        }

        private QueryRsp<List<BsOptLog>> logsrsp = new QueryRsp<List<BsOptLog>>();

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
            this.logsrsp = BsConfig.GetOptLogs(this.pageIndex, pageSize,this.optUserId);
            isloading = false;
            await base.InvokeAsync(base.StateHasChanged);
        }
    }
}