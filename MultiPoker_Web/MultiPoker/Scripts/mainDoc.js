$(document).ready(function () {

    var lastDate;

    $('#bonus').click(function () {
        document.getElementById('bonus').disabled = "disabled";
        $.ajax({
            url: "Home/GetBonus",
            type: "post",
            data: {},
            success: function (data) {
                if (data != null || data != "")
                {
                    var elem = $('#bonus');
                    var label = document.createElement('label');
                    label.setAttribute('id', 'bns');
                    label.innerHTML = '+'+data;
                    label.setAttribute('style', 'position:absolute; margin-left:-' + elem.offset().left * 2 + 'px; margin-top:' + elem.offset().top / 2 + 'px; font-size:100pt; background-color:transparent; color:yellow;');
                    label.classList.add('text-center');

                    $('#mainIndexHome').append(label);

                    $('#bns').animate({ marginTop: 10 }, {
                        duration: 2000,
                        step: function (cv, fx) {
                            $(this).css({});
                        },
                        complete: function () {
                            document.getElementById(this.id).remove();
                        }
                    });

                    lastDate = new Date();
                    lastDate.setHours(new Date().getHours() + 4);
                    startTime();
                }
            },
            error:function(data)
            {
                alert("Error");
                document.getElementById('bonus').disabled = "";
            }
        });


    });

    $.ajax({
        url: "Home/GetTime",
        type: "post",
        data: {},
        success: function (data) {
            if (data != "") {
                data = data.split("|");
                lastDate = new Date(data[0], data[1] - 1, data[2], data[3], data[4], data[5], 0);
                lastDate.setHours(lastDate.getHours() + 4);
                startTime();
            }
        }
    });

    function startTime() {
        var diff;
        var date = new Date();
        var realSeconds = parseInt(date.getTime() - lastDate.getTime()) / 1000;

        var hours = parseInt(realSeconds / 3600) * -1;
        var minutes = parseInt(realSeconds / 60 % 60) * -1;
        var seconds = parseInt(realSeconds % 60) * -1;

        //кол-во секунд, если число больше нуля то прошло 4 часа; 4 часа = 14400
        var diff = realSeconds % 14400;
    
        if (hours < 10) hours = "0" + hours;
        if (minutes < 10) minutes = "0" + minutes;
        if (seconds < 10) seconds = "0" + seconds;

        if (diff > 0)
        {
            document.getElementById("bonus").innerHTML = 'Free Chips';
            document.getElementById('bonus').disabled = "";
        }
        else
        {
            document.getElementById('bonus').disabled = "disabled";
            document.getElementById("bonus").innerHTML = hours + ":" + minutes + ":" + seconds;
            setTimeout(startTime, 1000);
        }
    }

});