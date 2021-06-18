using System;

namespace BasicBoard.Models
{
    public class PageDTO
    {
        public int startPage { get; set; } //시작페이지
        public int endPage { get; set; } //끝페이지
        public Boolean prev { get; set; } //이전페이지로 이동
        public Boolean next { get; set; } //다음페이지로 이
        public int total { get; set; } //전체 게시물 갯
        public Criteria cri { get; set; } 

        public PageDTO(Criteria cri, int total)
        {
            this.cri = cri;
            this.total = total;

            this.endPage = (int)Math.Ceiling(cri.pageNum / 10.0) * 10; 

            this.startPage = this.endPage - 9; //만약 끝페이지가 10이면 첫페이지는 1

            int realEnd = (int)(Math.Ceiling((total * 1.0) / cri.amount));

            if(realEnd < this.endPage)
            {
                this.endPage = realEnd;
            }

            this.prev = this.startPage > 1;
            this.next = this.endPage < realEnd;
        }
    }
}
