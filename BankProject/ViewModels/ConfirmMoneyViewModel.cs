namespace BankProject.ViewModels
{
    public class ConfirmMoneyViewModel
    {
        public string SenderAccountNumber { get; set; }
        public string ReceiverAccountNumber { get; set; }
        public string senderName { get; set; }
        public string ReciverName { get; set; }
        public string ReceiverBankName { get; set; }
        public string SenderBankName { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string TransactionPassword { get; set; }
    }
}
