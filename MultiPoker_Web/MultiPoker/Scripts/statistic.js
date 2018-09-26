$(document).ready(function () {

    //обработчик select для статистике по выбранной игре
    $('#gameId').change(function () {
        var game_id = $(this).val();
        game_id = parseInt(game_id, 10);

        $.ajax({
            url:"StatByGame",
            type:"post",
            data: { gameid: game_id },
            success: function (data) {
                if(data != null)
                {
                    var str = data.split('|');

                    $('#totalGames').val(str[0]);
                    $('#wins').val(str[1]);
                    $('#maxGain').val(str[2]);
                    if (str[3] != "")
                    {
                        $('#c1').attr("src", str[3]);
                        $('#c2').attr("src", str[4]);
                        $('#c3').attr("src", str[5]);
                        $('#c4').attr("src", str[6]);
                        $('#c5').attr("src", str[7]);
                        $('#combination').val(str[8]);
                    }
                    else
                    {
                        $('#c1').attr("src", "/Images/BlackBack.png");
                        $('#c2').attr("src", "/Images/BlackBack.png");
                        $('#c3').attr("src", "/Images/BlackBack.png");
                        $('#c4').attr("src", "/Images/BlackBack.png");
                        $('#c5').attr("src", "/Images/BlackBack.png");
                        $('#combination').val("No Result");
                    }
                }
            },
            error: function (data) {
                $('#totalGames').val(0);
                $('#wins').val(0);
                $('#maxGain').val(0);

                $('#c1').attr("src", "/Images/BlackBack.png");
                $('#c2').attr("src", "/Images/BlackBack.png");
                $('#c3').attr("src", "/Images/BlackBack.png");
                $('#c4').attr("src", "/Images/BlackBack.png");
                $('#c5').attr("src", "/Images/BlackBack.png");

                $('#combination').val("No Result");
            }
        });
    });

});