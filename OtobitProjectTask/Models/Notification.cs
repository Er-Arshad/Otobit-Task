namespace OtobitProjectTask.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string ConnectionId { get; set; }
        public string Message { get; set; }
        public int SenderId { get; set; }
    }
}
