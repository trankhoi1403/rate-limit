using System;

namespace RateLimit.Core.BUS.Models
{
    public class RequestIpHistoryModel
    {
        /// <summary>
        /// địa chỉ ip
        /// </summary>
        public string IpAddress { get; set; }
        /// <summary>
        /// Lần đầu được cấp phép cho qua, request sau khi hết hạn blacklist sẽ được set lại
        /// </summary>
        public DateTime FirstApprovedRequest { get; set; } = DateTime.MinValue;
        /// <summary>
        /// Số lượng approved request tính từ thời điểm FirstApprovedRequest
        /// </summary>
        public int NumberOfApprovedRequest { get; set; } = 0;
        /// <summary>
        /// Thời gian hết hạn blacklist
        /// </summary>
        public DateTime ReleaseTime { get; set; } = DateTime.MinValue;

        public void Lock(int secondFromNow)
        {
            this.ReleaseTime = DateTime.Now.AddSeconds(secondFromNow);
        }

        public void Unlock()
        {
            this.FirstApprovedRequest = DateTime.UtcNow;
            this.NumberOfApprovedRequest = 1;
            this.ReleaseTime = DateTime.MinValue;
        }
    }
}
