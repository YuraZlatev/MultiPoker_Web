function getRooms(bt)
{
    var type = "";
    var blind = "Big Blind ";
    switch(bt.id)
    {
        case "easy": type = "easy"; blind += "200"; break;
        case "normal": type = "normal"; blind += "1000"; break;
        case "hard": type = "hard"; blind += "2000"; break;
    }

    $.ajax({
        url: "GetRoomsByType",
        type: "post",
        data: { type: type, game : $('#gameName').text() },
        success: function (data) {
            $('#nameOfBet').html('<h3 style="font-weight:bold; color:aqua;" class="text-center">'+blind+'</h3>');
            
            var div = '<ul style="list-style-type:none;" class="text-center">';
            for (i = 0; i<data.length; i++)
            {
                var places = 0;
                for (p = 0; p < data[i].Places.length; p++)
                {
                    if (data[i].Places[p] == 1)
                        places++;
                }

                var butt = '<li><button onmouseover="Over(this)" onmouseout="Out(this)" onclick="startPlay(this)"' +
                    'style="margin-top:10px; border:1px solid lime; border-radius:10px; background-color:black; color:orange;" id="room_' + data[i].RoomId + '">';
                butt += "Room №" + data[i].RoomId + '&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;' + places + "/" + data[i].Places.length;
                butt += '</button></li>';

                div += butt;
            }
            div += "</ul>";

            $('#roomsbybet').html(div);
        }
    });
}

function Over(bt)
{
    bt.style.color = "red";
    bt.style.border = "1px solid aqua";
}

function Out(bt)
{
    bt.style.color = "orange";
    bt.style.border = "1px solid lime";
}

function startPlay(bt)
{
    var roomId = parseInt(bt.id.substr(5));
    var game = $('#gameName').text();

    location = location.origin + "/Rooms/Index?room_id=" + roomId+"&game="+game;
}