$(document).ready(function () {

    var game = $.connection.pokerHub;

    game.client.lostPlace = function () { alert("Your balance is less than big blind of room ! You cannot play."); }
    
    game.client.clearPage = function () { clearPage(); }

    game.client.newMessage = function (player, msg) { newMessage(player, msg); }

    game.client.placeTrue = function () { placeTrue(); };

    game.client.showExp = function (exp) { showExp(exp);}

    //новый игрок который выбрал себе игровое место
    game.client.newPlayer = function (p) { newPlayer(p); };
    //удаление игрока, который вышел из игры
    game.client.removePlayer = function (p) { removePlayer(p); }

    game.client.animatedCard = function (player, card) { animatedCard(player, card); }

    game.client.setBet = function (player, bet, bankIndex, bank) { setBet(player, bet, bankIndex, bank); }

    //анимирование ставок в банк
    game.client.collectBets = function () { collectBets(); }

    game.client.openCards = function (player) { openCards(player); }

    game.client.result = function (player, index, win, cards, combinationName) { result(player, index, win, cards, combinationName); }

    //активация / диактивация интерфейса
    game.client.activation = function (flag, player, currentBet)
    {
        if (flag == true)
        {
            $('#currentBetOfRoom').val(currentBet);
            $('#interface').css("visibility", "visible");
            createSlider(player);
        }
        else
            $('#interface').css("visibility", "collapse");
    }

    $.connection.hub.start().done(function () {
        
        //занять свободное место за столом-----------
        $('#plus_1').click(function () { getInfo(0); });
        $('#plus_2').click(function () { getInfo(1); });
        $('#plus_3').click(function () { getInfo(2); });
        $('#plus_4').click(function () { getInfo(3); });
        $('#plus_5').click(function () { getInfo(4); });
        $('#plus_6').click(function () { getInfo(5); });
        $('#plus_7').click(function () { getInfo(6); });
        $('#plus_8').click(function () { getInfo(7); });
        $('#plus_9').click(function () { getInfo(8); });
        //-----------------------------------

        //обработка кнопок интерфейса---------
        $('#re-raise').click(function () { kindOfButton('re-raise'); });
        $('#raise').click(function () { kindOfButton('raise'); });
        $('#call').click(function () { kindOfButton('call'); });
        $('#check').click(function () { kindOfButton('check'); });
        $('#fold').click(function () { kindOfButton('fold'); });
        $('#all-in').click(function () { kindOfButton('all-in'); });

        function kindOfButton(kind)
        {
            $('#interface').css("visibility", "collapse");
            var curBet = parseInt($('#currentBetOfRoom').val());
            var roomid = parseInt($('#room').val());
            var myBet = parseInt(getInterface(parseInt($('#placeNumber').val())).children[2].children[1].innerHTML);
            switch(kind)
            {
                case 're-raise':
                    var reraiseValue = parseInt($('#currentBet').html());
                    var myBalance = 0;
                    var elem = getInterface(parseInt($('#placeNumber').val()));
                    switch (elem.id)
                    {
                        case "int_1": myBalance = parseInt(elem.children[0].children[3].value); break;
                        case "int_2": myBalance = parseInt(elem.children[0].children[3].value); break;
                        case "int_3": myBalance = parseInt(elem.children[0].children[3].value); break;
                        case "int_4": myBalance = parseInt(elem.children[0].children[3].value); break;
                        default: myBalance = parseInt(elem.children[0].children[2].value); break;
                    }

                    if (reraiseValue >= myBalance)
                    {
                        alert('Your balance is not enough to Re-Raise. If the same - you may push "All-In"');
                        $('#interface').css("visibility", "visible");
                    }
                    else
                    {
                        //метод на сервера для обработки Re-Raise
                        game.server.kindOfResponse('re-raise', roomid, reraiseValue);
                    }

                    break;
                case 'raise':
                    if (curBet == myBet)
                    {                    
                        //метод на сервера для обработки Raise на величину Big Blind за столом
                        game.server.kindOfResponse('raise', roomid, -1);
                    }
                    else
                    {
                        alert('Now you cannot push "Raise"');
                        $('#interface').css("visibility", "visible");
                    }

                    break;
                case 'call':
                    var elem = getInterface(parseInt($('#placeNumber').val()));
                    var curMoney = null;
                    switch(elem.id)
                    {
                        case 'int_1': curMoney = elem.children[0].children[3].value; break;
                        case 'int_2': curMoney = elem.children[0].children[3].value; break;
                        case 'int_3': curMoney = elem.children[0].children[3].value; break;
                        case 'int_4': curMoney = elem.children[0].children[3].value; break;
                        default: curMoney = elem.children[0].children[2].value; break;
                    }
                    if(curBet == 0 || curBet == myBet)
                    {
                        alert('Bet is 0 or the same as your bet. Push another button');
                        $('#interface').css("visibility", "visible");
                    }
                    else if (curMoney >= curBet)
                    {
                        //метод на сервера для обработки Call
                        game.server.kindOfResponse('call', roomid, -1);
                    }
                    else
                    {
                        alert('Push "All-In"');
                        $('#interface').css("visibility", "visible");
                    }

                    break;
                case 'check':
                    if (curBet == myBet)
                    {
                        //метод на сервера для обработки Check
                        game.server.kindOfResponse('check', roomid, -1);
                    }
                    else
                    {
                        alert('Now you cannot push "Check"');
                        $('#interface').css("visibility", "visible");
                    }

                    break;
                case 'fold':
                    //метод на сервера для обработки Fold
                    game.server.kindOfResponse('fold', roomid, -1);
                    break;
                case 'all-in':
                    //метод на сервера для обработки All-In
                    game.server.kindOfResponse('all-in', roomid, -1);
                    break;
            }
        }
        //-------------------------------

        $('#sit').click(function () {
            //проверка на входящую сумму
            var min = parseInt($('#min').html());
            var balance = parseInt($('#balance').html());
            if (min <= balance)
                game.server.takePlace(parseInt($('#curPlace').val()), $('#room').val(), parseInt($('#min').html()));
            else
                alert("Your balance is less than the amount selected !");
        });
        
        $('#send').click(function () {
            var msg = document.getElementById('message');
            var message = $.trim(msg.value);
            if (message == "")
            {
                msg.value = "";
                return;
            }
         
            game.server.sendMessage(parseInt($('#room').val()), parseInt($('#placeNumber').val()), message);
            msg.value = "";
        });
        
    });

    updatePage();
    //обновить даные по комнате
    function updatePage()
    {
        $.ajax({
            url: "Info",
            type: "post",
            data: { room_id: parseInt($('#room').val()) },
            success: function (obj) {
                var room = obj["room"];
                //заполнение данных об игроках за столом
                for(var i = 0; i < room.Places.Length; i++)
                {
                    var player = room.PlayerByPlace(i);
                    if (player == null)
                        continue;

                    var elem = getInterface(i);
                    switch (elem.id)
                    {
                        case "int_1": elem.children[0].children[3].value = player.Game.CurrentBalance; break;
                        case "int_2": elem.children[0].children[3].value = player.Game.CurrentBalance; break;
                        case "int_3": elem.children[0].children[3].value = player.Game.CurrentBalance; break;
                        case "int_4": elem.children[0].children[3].value = player.Game.CurrentBalance; break;
                        default: elem.children[0].children[2].value = player.Game.CurrentBalance; break;
                    }
                    var cards = player.Game.Cards;
                    if (cards != null && cards.Count == 2)
                    {
                        elem.children[1].children[0].setAttribute('src', cards[0].ImagePath);
                        elem.children[1].children[1].setAttribute('src', cards[1].ImagePath);
                    }
                    elem.children[2].children[1].innerText = player.Game.Bet;
                    if (player.Game.Bet != 0)
                        elem.children[2].style.visibility = "visible";
                    else
                        elem.children[2].style.visibility = "collapse";
                }
                //заполнение существующих банков
                for(var i = 0; i < room.Bank.Count; i++)
                {
                    var elem = document.getElementById('bank' + room.Bank[i].Index);
                    elem.style.visibility = "visible";
                    elem.children[1].innerHTML = room.Bank[i].CurrentBank;
                }
                //заполнение 5 общих карт
                for(var i = 0; i < room.Cards.Count; i++)
                {
                    document.getElementById('fiveCards').children[i].setAttribute('src', room.Cards[i].ImagePath);
                }
            }
        });
    }
    
    changeColor();
    //при загрузке страницы покрасить border игроков в красный и себя в зеленый
    function changeColor()
    {
        var myPlace = $('#placeNumber').val();
        for(i = 0; i < 9; i++)
        {
            var elem = getInterface(i);
            if (elem == null || elem.children.length == 1) //если div не отображается или виден только +
                continue;

            if(i == myPlace)
            {
                elem.style.borderColor = 'rgb(34,255,0)';
                $('#' + elem.id).removeClass('intClass');
                $('#' + elem.id).addClass('intClass2');
            }
            else
            {
                elem.style.borderColor = 'rgb(255,0,0)';
                $('#' + elem.id).removeClass('intClass');
                $('#' + elem.id).addClass('intClass2');
            }
        }
    }
});


function clearPage()
{
    document.getElementById('interface').style.visibility = "collapse";
    //очистка значений банков
    document.getElementById('bank1').style.visibility = "collapse";
    document.getElementById('bank1').children[1].innerHTML = 0;
    document.getElementById('bank2').style.visibility = "collapse";
    document.getElementById('bank2').children[1].innerHTML = 0;
    document.getElementById('bank3').style.visibility = "collapse";
    document.getElementById('bank3').children[1].innerHTML = 0;
    document.getElementById('bank4').style.visibility = "collapse";
    document.getElementById('bank4').children[1].innerHTML = 0;
    document.getElementById('bank5').style.visibility = "collapse";
    document.getElementById('bank5').children[1].innerHTML = 0;
    document.getElementById('bank6').style.visibility = "collapse";
    document.getElementById('bank6').children[1].innerHTML = 0;
    document.getElementById('bank7').style.visibility = "collapse";
    document.getElementById('bank7').children[1].innerHTML = 0;
    document.getElementById('bank8').style.visibility = "collapse";
    document.getElementById('bank8').children[1].innerHTML = 0;
    //очистка общих карт
    var fiveCards = document.getElementById('fiveCards');
    for (var i = 0; i < fiveCards.children.length; i++)
        fiveCards.children[i].removeAttribute('src');
    //очистка карт каждого игрока
    for(var i = 0; i < 9; i++)
    {
        var elem = getInterface(i);
        if (elem == null)
            continue;
        elem.children[1].children[0].removeAttribute('src');
        elem.children[1].children[1].removeAttribute('src');

        elem.children[2].style.visibility = "collapse";
        elem.children[2].children[1].innerHTML = 0;
    }
}
//end of document.ready --------------------------------------------------------------------

function showExp(exp)
{
    var place = parseInt($('#placeNumber').val());
    var myElem = getInterface(place);

    var top = $("#" + myElem.id).offset().top - $('#main').offset().top;
    var left = $("#" + myElem.id).offset().left - $('#main').offset().left;

    var lb = document.createElement('label');
    lb.setAttribute('id', 'myExp');
    lb.innerHTML = '+' + exp + 'exp';
    lb.setAttribute('style', 'position:absolute; margin-left:'+left+'px; margin-top:'+top+'px; color:aqua; font-size:14pt;');

    $('#main').append(lb);
    $('#' + lb.id).animate({ marginTop: top - 50 }, {
        duration: 1000,
        step: function (cv, fx) {
        },
        complete: function () {
            document.getElementById(this.id).remove();
        }
    });
}

//отображения входящих сообщений от игроков
function newMessage(player, msg)
{
    var elem = document.getElementById('allMessages').children[0];

    var p = document.createElement('p');
    p.innerText = player.Nick + " :";
    var span = document.createElement('span');
    span.innerText = msg;
    span.setAttribute('style', 'color:white; padding:10px;');

    var div = document.createElement('div');

    if (player.Game.Place == -1)
    {
        p.style.color = "silver";
        div.setAttribute('style', 'padding:10px; border:1px solid transparent; border-bottom-color:silver; width:90%; margin-left:5%;');
    }
    else if (player.Game.Place == parseInt($('#placeNumber').val()))
    {
        p.style.color = "lime";
        div.setAttribute('style', 'padding:10px; border:1px solid transparent; border-bottom-color:lime; width:90%; margin-left:5%;');
    }
    else
    {
        p.style.color = "red";
        div.setAttribute('style', 'padding:10px; border:1px solid transparent; border-bottom-color:red; width:90%; margin-left:5%;');
    }

    div.appendChild(p);
    div.appendChild(span);

    elem.appendChild(div);
}

function getInfo(val)
{
    $.ajax({
        url: "Info",
        type: "post",
        data: { room_id: parseInt($('#room').val()) },
        success: function (obj) {
            $('#curPlace').val(val);
            $('#min').html(obj["room"].MinimumBalance);
            $('#max').html(obj["room"].MaximumBalance);
            $('#balance').html(obj["player"].Balance);

            $("#sitSlider").slider({
                value: obj["room"].MinimumBalance,
                min: obj["room"].MinimumBalance,
                max: obj["room"].MaximumBalance,
                step: 1000,
                slide: function (event, ui) { $('#min').html(ui.value); }
            });

            $('#inputMoney').click();
        }
    });
}

//slider - элемент интерфейса игрока
function createSlider(player)
{
    var curBet = parseInt($('#currentBetOfRoom').val());
    var myBet = player.Game.Bet;
    var minValue = curBet - myBet + parseInt($('#currentBigBlind').val());

    $('#currentBet').html(minValue);
    $("#slider").slider({
        value: minValue,
        min: minValue,
        max: player.Game.CurrentBalance,
        step: 100,
        slide: function (event, ui) {  $('#currentBet').html(ui.value); }
    });
}


function placeTrue()
{
    $('#myPlace').val("True");
    if ($('#int_1').children().length == 1)
        $('#int_1').remove();
    if ($('#int_2').children().length == 1)
        $('#int_2').remove();
    if ($('#int_3').children().length == 1)
        $('#int_3').remove();
    if ($('#int_4').children().length == 1)
        $('#int_4').remove();
    if ($('#int_5').children().length == 1)
        $('#int_5').remove();
    if ($('#int_6').children().length == 1)
        $('#int_6').remove();
    if ($('#int_7').children().length == 1)
        $('#int_7').remove();
    if ($('#int_8').children().length == 1)
        $('#int_8').remove();
    if ($('#int_9').children().length == 1)
        $('#int_9').remove();
}

function getInterface(place)
{
    var elem = null;
    switch(place)
    {
        case 1: elem = document.getElementById('int_2'); break;
        case 2: elem = document.getElementById('int_3'); break;
        case 3: elem = document.getElementById('int_4'); break;
        case 4: elem = document.getElementById('int_5'); break;
        case 5: elem = document.getElementById('int_6'); break;
        case 6: elem = document.getElementById('int_7'); break;
        case 7: elem = document.getElementById('int_8'); break;
        case 8: elem = document.getElementById('int_9'); break;
        default: elem = document.getElementById("int_1"); break;
    }

    return elem;
}

function newPlayer(player) {
    $.ajax({
        url: "GetInterfaceByPlace",
        type: "post",
        data: { player: player, flag : true},
        success: function (partial) {
            var elem = getInterface(player.Game.Place);
            if (elem != null)
            {
                elem.remove();
            }
            $('#playerPlace').append(partial);
            
            var mySelf = $('#userid').val();
            elem = getInterface(player.Game.Place);
            $('#' + elem.id).removeClass('intClass');
            $('#' + elem.id).addClass('intClass2');

            if (player.Id == mySelf)
            {
                elem.style.borderColor = 'rgb(34,255,0)'; //lime
                $('#placeNumber').val(player.Game.Place);
                createSlider(player);
            }
            else
                elem.style.borderColor = 'rgb(255,0,0)';  //red
        }
    });
}

function removePlayer(player) {
    $.ajax({
        url: "GetInterfaceByPlace",
        type: "post",
        data: { player: player, flag: false },
        success: function (partial) {
            if (partial == null)
                return;

            var elem = getInterface(player.Game.Place);
            if (elem != null)
            {
                elem.remove();
            }
            if ($('#myPlace').val() == "False")
            {
                $('#playerPlace').append(partial);
            }
        }
    });
}

//открывает карты данного игрока
function openCards(player)
{
    var elem = getInterface(player.Game.Place);
    if (elem == null)
        return;

    var cards = player.Game.Cards;
    elem.children[1].children[0].setAttribute('src', cards[0].ImagePath);
    elem.children[1].children[1].setAttribute('src', cards[1].ImagePath);
}


var bankNumber = 0;
function result(player, index, win, cards, combinationName)
{
    bankNumber++;
    if (bankNumber >= 100)
        bankNumber = 0;

    var elem = getInterface(player.Game.Place);
    switch (elem.id)
    {
        case "int_1": elem.children[0].children[3].value = player.Game.CurrentBalance; break;
        case "int_2": elem.children[0].children[3].value = player.Game.CurrentBalance; break;
        case "int_3": elem.children[0].children[3].value = player.Game.CurrentBalance; break;
        case "int_4": elem.children[0].children[3].value = player.Game.CurrentBalance; break;
        default: elem.children[0].children[2].value = player.Game.CurrentBalance; break;
    }

    var serverDiv = document.createElement('div');
    serverDiv.setAttribute('style', 'padding:10px; border:1px solid transparent; border-bottom-color:aqua; width:90%; margin-left:5%;');
    var p = document.createElement('p');
    p.setAttribute('style', 'color:white;');
    p.innerHTML = player.Nick + " win: " + win;

    if(combinationName != null)
    {
        p.innerHTML += ", with ";
        serverDiv.appendChild(p);

        for (var i = 0; i < 5; i++)
        {
            var img = document.createElement('img');
            img.setAttribute('width', '40');
            img.setAttribute('height', '60');
            img.setAttribute('src', cards[i].ImagePath);
            img.setAttribute('style', 'margin-left:10px; display:inline;');
            serverDiv.appendChild(img);
        }
        serverDiv.appendChild(document.createElement('br'));

        var name = document.createElement('p');
        name.setAttribute('style', 'color:white; padding-top:5px;');
        name.classList.add('text-center');
        name.innerHTML = combinationName;
        serverDiv.appendChild(name);
    }
    else
        serverDiv.appendChild(p);

    document.getElementById('allMessages').children[0].appendChild(serverDiv);


    var banks = document.getElementById('allBanks');

    var bankLeft =  parseInt(banks.style.width) / 2 + parseInt(banks.style.marginLeft) - 100;
    var bankTop = parseInt(banks.style.height) / 2 + parseInt(banks.style.marginTop);

    var div = document.createElement('div');
    div.setAttribute('id', 'divBank' + bankNumber);
    div.setAttribute('style', 'position:absolute; margin-left:' + bankLeft + 'px; margin-top:' + bankTop + 'px;');
    var img = document.createElement('img');
    img.setAttribute('style', 'position:relative');
    img.setAttribute('width', '20');
    img.setAttribute('height', '20');
    img.setAttribute('src', '/Images/Chip.png');
    var label = document.createElement('label');
    label.classList.add('bank');
    label.innerHTML = win;
    div.appendChild(img);
    div.appendChild(label);

    document.getElementById('bank' + index).style.visibility = "collapse";
    $('#main').append(div);
    var top = $("#" + elem.id).children().eq(2).children().eq(1).offset().top - $('#main').offset().top;
    var left = $("#" + elem.id).children().eq(2).children().eq(1).offset().left - $('#main').offset().left;

    $('#' + div.id).animate({ marginLeft: left, marginTop: top }, {
        duration: 1000,
        step: function (cv, fx) {

        },
        complete: function () {
            document.getElementById(this.id).remove();
        }
    });
}

//обновления данных о игроке и о текущей ставки в игре
function setBet(player, bet, bankIndex, bank)
{
    $('#currentBetOfRoom').val(bet);
    var elem = getInterface(player.Game.Place);
    switch(elem.id)
    {
        case "int_1": elem.children[0].children[3].value = player.Game.CurrentBalance; break;
        case "int_2": elem.children[0].children[3].value = player.Game.CurrentBalance; break;
        case "int_3": elem.children[0].children[3].value = player.Game.CurrentBalance; break;
        case "int_4": elem.children[0].children[3].value = player.Game.CurrentBalance; break;
        default: elem.children[0].children[2].value = player.Game.CurrentBalance; break;
    }  
    elem.children[2].children[1].innerText = player.Game.Bet;
    if(bet != 0)
        elem.children[2].style.visibility = "visible";
    else
        elem.children[2].style.visibility = "collapse";

    if(bankIndex != null && bank != null)
        updateBank(bankIndex, bank);
}
function updateBank(bankIndex, bank)
{
    switch(bankIndex)
    {
        case 1: document.getElementById('bank1').children[1].innerHTML = bank; document.getElementById('bank1').style.visibility = "visible"; break;
        case 2: document.getElementById('bank2').children[1].innerHTML = bank; document.getElementById('bank2').style.visibility = "visible"; break;
        case 3: document.getElementById('bank3').children[1].innerHTML = bank; document.getElementById('bank3').style.visibility = "visible"; break;
        case 4: document.getElementById('bank4').children[1].innerHTML = bank; document.getElementById('bank4').style.visibility = "visible"; break;
        case 5: document.getElementById('bank5').children[1].innerHTML = bank; document.getElementById('bank5').style.visibility = "visible"; break;
        case 6: document.getElementById('bank6').children[1].innerHTML = bank; document.getElementById('bank6').style.visibility = "visible"; break;
        case 7: document.getElementById('bank7').children[1].innerHTML = bank; document.getElementById('bank7').style.visibility = "visible"; break;
        case 8: document.getElementById('bank8').children[1].innerHTML = bank; document.getElementById('bank8').style.visibility = "visible"; break;
    }
}

function collectBets()
{
    for(var i = 0; i < 9; i++)
    {
        var elem = getInterface(i);
        if (elem == null)
            continue;
        //если ставки нет - пропуск
        if (parseInt(elem.children[2].children[1].innerHTML) == 0)
            continue;

        elem.children[2].style.visibility = "collapse";      

        var top = $("#"+elem.id).children().eq(2).children().eq(1).offset().top - $('#main').offset().top;
        var left = $("#" + elem.id).children().eq(2).children().eq(1).offset().left - $('#main').offset().left;

        var div = document.createElement('div');
        div.setAttribute('id', 'div' + i.toString());
        div.setAttribute('style', 'position:absolute; margin-left:'+left+'px; margin-top:'+top+'px;');
        var img = document.createElement('img');
        img.setAttribute('style', 'position:relative;');
        img.setAttribute('width', '20');
        img.setAttribute('height', '20');
        img.setAttribute('src', '/Images/Chip.png');
        var label = document.createElement('label');
        label.classList.add('bank');
        label.innerHTML = elem.children[2].children[1].innerHTML;
        div.appendChild(img);
        div.appendChild(label);

        $('#main').append(div);
        var banks = document.getElementById('allBanks');
        var banksLeft = parseInt(banks.style.width) / 2 + parseInt(banks.style.marginLeft) - 100;
        var banksTop = parseInt(banks.style.height) / 2 + parseInt(banks.style.marginTop);

        $('#' + div.id).animate({ marginLeft: banksLeft, marginTop: banksTop}, {
            duration: 1000,
            step: function (cv, fx) {

            },
            complete: function () {
                document.getElementById(this.id).remove();
            }
        });
    }
}

var stack = [];
var cardsCount = 0;

var fiveCardsStack = [];
var fiveCardsCount = 0;
//анимация карты по игроку или если player == null, то добавление в общие карты
function animatedCard(player, card)
{
    if (player != null)
    {
        //интерфейс данного игрока
        var elem = getInterface(player.Game.Place);
        var src = elem.children[1].children[0].getAttribute('src');
        var imagePath = card.ImagePath;

        var img = document.createElement('img');
        img.setAttribute('id', 'animCard' + cardsCount);
        img.setAttribute('style', 'position:absolute; margin-left:570px; margin-top:10px; transform:scale(0.3, 0.3);');
        img.setAttribute('src', '/Images/BlackBack.png');
        img.setAttribute('width', '60');
        img.setAttribute('height', '100');

        stack.push({ "0": elem, "1": img, "2": imagePath, "3": src, "4":player.Game.Place });

        cardsCount++;
        if (cardsCount >= 100)
            cardsCount = 0;

        $('#main').append(img);

        $('#'+img.id).animate({ marginLeft: elem.style.marginLeft, marginTop: elem.style.marginTop, cv:"1.0"}, {
            duration: 500,
            step: function (cv, fx) {
                $(this).css({
                    "-webkit-transform": "scale(" + cv + ", " + cv + ")",
                    "-moz-transform": "scale(" + cv + ", " + cv + ")",
                    "transform": "scale(" + cv + ", " + cv + ")"
                });
            },
            complete: function () {             
                var obj = stack.shift();

                if (obj["3"] == "" || obj["3"] == null)
                {
                    if(obj["4"] == $('#placeNumber').val())
                        obj["0"].children[1].children[0].setAttribute('src', obj["2"]);
                    else
                        obj["0"].children[1].children[0].setAttribute('src', '/Images/BlackBack.png');
                }
                else
                {
                    if (obj["4"] == $('#placeNumber').val())
                        obj["0"].children[1].children[1].setAttribute('src', obj["2"]);
                    else
                        obj["0"].children[1].children[1].setAttribute('src', '/Images/BlackBack.png');
                }
                
                document.getElementById(obj["1"].id).remove();
            }
        });
    }
    else //если карта общая
    {
        var elem = document.getElementById('fiveCards');
        var imagePath = card.ImagePath;

        var img = document.createElement('img');
        img.setAttribute('id', 'fiveCard' + fiveCardsCount);
        img.setAttribute('style', 'position:absolute; margin-left:550px; margin-top:10px; transform:scale(0.3, 0.3);');
        img.setAttribute('src', '/Images/BlackBack.png');
        img.setAttribute('width', '60');
        img.setAttribute('height', '100');

        fiveCardsStack.push({ "0": fiveCardsCount, "1": img, "2": imagePath});

        fiveCardsCount++;
        var dist = 70 * (fiveCardsCount - 1) + parseInt(elem.style.marginLeft);
        if (fiveCardsCount > 4)
            fiveCardsCount = 0;

        $('#main').append(img);

        $('#'+img.id).animate({ marginLeft: dist, marginTop: elem.style.marginTop, cv: "1.0" }, {
            duration: 1000,
            step: function (cv, fx) {
                $(this).css({
                    "-webkit-transform": "scale(" + cv + ", " + cv + ")",
                    "-moz-transform": "scale(" + cv + ", " + cv + ")",
                    "transform": "scale(" + cv + ", " + cv + ")"
                });
            },
            complete: function () {
                var obj = fiveCardsStack.shift();

                var childrenCount = parseInt(obj["0"]);
                var image = obj["1"];
                var src = obj["2"];
                
                document.getElementById('fiveCards').children[childrenCount].setAttribute('src', src);

                document.getElementById(image.id).remove();
            }
        });
    }
}