namespace WPR.Controllers.vehicleManager.ChangeUserEmailAndPasswordController
{
    /// <summary>
    /// DTO for updating user credentials.
    /// </summary>
    public class UpdateUserCredentialsRequest
    {
        public int UserId { get; set; }
        public string NewEmail { get; set; }
        public string NewPassword { get; set; }
        public string BusinessCode { get; set; }
    }
}
