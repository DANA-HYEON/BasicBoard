console.log("Reply Module....");

var replyService = (function () {

    //댓글 추가
    function add(reply, callback, error) {
        console.log("add reply....");

        $.ajax({
            type: "POST",
            url: "/reply/new",
            data: JSON.stringify(reply),
            contentType: "application/json; charset=utf-8",
            success: function (result, status, xhr) {
                if (callback) {
                    callback(result)
                }
            },
            error: function (xhr, status, er) {
                if (error) {
                    error(er);
                }
            }
        });
    }

    //댓글리스트
    function getList(param, callback, error) {
        var bno = param.bno;

        $.getJSON("/reply/list/" + bno,
            function (data) {
                if (callback) {
                    callback(data);
                }
            }).fail(function (xhr, status, err) {
                if (error) {
                    error();
                }
            });
    }

    //댓글삭제
    function remove(rno, callback, error) {
        $.ajax({
            type: "DELETE",
            url: "/reply/" + rno,
            success: function (deleteResult, status, xhr) {
                if (callback) {
                    callback(deleteResult);
                }
            },
            error: function (xhr, status, er) {
                if (error) {
                    error(er);
                }
            }
        });
    }

    //댓글수정
    function update(reply, callback, error) {
        console.log("rno : " + reply.replyNo);

        $.ajax({
            type: "PUT",
            url: "/reply/" + reply.replyNo,
            data: JSON.stringify(reply),
            contentType: "application/json; charset=utf-8",
            success: function (result, satatus, xhr) {
                if (callback) {
                    callback(result);
                }
            },
            error: function (xhr, status, er) {
                if (error) {
                    error(er);
                }
            }
        });
    }


    //댓글 등록 날짜 표현 함수
    function displayTime(timeValue) {
        var today = new Date();
        var timeStamp = new Date(timeValue); //DB의 timeStamp 형식을 Date로 변환
        var gap = today.getTime() - timeStamp;

        var dateObj = new Date(timeStamp);
        var str = "";

        if (gap < (1000 * 60 * 60 * 24)) { //댓글을 작성한 날로부터 24시간이 안지나면 시간으로 표시
            var hh = dateObj.getHours();
            var mi = dateObj.getMinutes();
            var ss = dateObj.getSeconds();

            return [(hh > 9 ? '' : '0') + hh, ":", (mi > 9 ? '' : '0') + mi, ':', (ss > 9 ? '' : '0') + ss].join('');
        } else { //24시간이 지나면 날짜로 표시
            var yy = dateObj.getFullYear();
            var mm = dateObj.getMonth() + 1; //getMonth()is zero-based
            var dd = dateObj.getDate();

            return [yy, '/', (mm > 9 ? '' : '0') + mm, '/', (dd > 9 ? '' : '0') + dd].join('');
        }
    };

    return {
        add: add,
        getList: getList,
        remove: remove,
        update: update,
        get: get,
        displayTime: displayTime
    };
})();