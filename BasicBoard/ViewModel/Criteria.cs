namespace BasicBoard.Models
{
    public class Criteria
    {
        public int pageNum { get; set; } //현재 페이지 번호

        public int amount { get; set; } // 한 페이지에 보여줄 컨텐츠의 갯수

        public string category { get; set; } //검색 카테고리

        public string searchString { get; set; } //검색 키워드


        public Criteria()
        {
            this.pageNum = 1;
            this.amount = 10;
        }
    }
}