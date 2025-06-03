using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlaScaf
{
    /// <summary>
    /// 列表查询的处理
    /// 同时也可给客户端软件使用
    /// </summary>
    public class QueryRsp<T>
    {
        [JsonPropertyName("success")]
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 对象值
        /// </summary>
        [JsonPropertyName("value")]
        public T Value { get; set; } = default(T);

        /// <summary>
        ///  查询总数
        /// </summary>
        [JsonPropertyName("total")]
        public int Total { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}