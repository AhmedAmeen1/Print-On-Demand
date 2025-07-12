namespace POD.DTO
{
    public class SellerProfileResponseDTO
    {
        public int SellerProfileId { get; set; }
        public string StoreName { get; set; }
        public string Description { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserId { get; set; }
    }
}
