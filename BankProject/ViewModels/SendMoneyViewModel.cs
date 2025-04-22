namespace BankProject.ViewModels
{
    public class SendMoneyViewModel
    {
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public int Senderid { get; set; }
     
        public int BankId { get; set; }
        public string Description { get; set; }
    }
}
