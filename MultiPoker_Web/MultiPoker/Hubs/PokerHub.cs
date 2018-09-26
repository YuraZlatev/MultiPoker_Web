using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Windows;
using System.Web.Mvc;
using MultiPoker.Controllers;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using MultiPoker.Models;
using System.Threading;

namespace MultiPoker.Hubs
{
    public class PokerHub : Hub
    {
        MultiPokerContext db = new MultiPokerContext();

        private int delay = 1000;   //задержка на действие
        private int delayCard = 500; //задержка для выдачи карты
        private int delayBets = 1500; //задержка для анимации ставок

        public void SendMessage(int roomid, int placeid, String msg)
        {
            Room room = MvcApplication.rooms[roomid];
            Player player = room.PlayerByPlace(placeid);
            Clients.Group(room.RoomId.ToString()).newMessage(player, msg);
        }
        public void TakePlace(int id, int roomid, int money)
        {
            Room room = MvcApplication.rooms[roomid];
            Player p = null;
            foreach(Player player in room.Players)
                if(player.Id == Context.User.Identity.GetUserId())
                {
                    room.Places[id] = 1;
                    player.Game.Place = id;
                    player.Game.RoomId = roomid;
                    player.Balance -= money;
                    player.Game.CurrentBalance = money;
                    try
                    {
                        Player pp = db.Players.Find(player.Id);
                        pp.Balance -= money;
                        db.SaveChanges();
                    }
                    catch { }

                    p = player;
                    break;
                }
            
            Clients.Caller.placeTrue();
            Clients.Group(roomid.ToString()).newPlayer(p);

            if(room.RoomGame == "Texas Holdem Poker")
                BeginPlayHoldem(room);
        }

        /// <summary>
        /// Ответ игрока
        /// </summary>
        /// <param name="answer">значение элемента интерфейса</param>
        /// <param name="roomid">id комнаты</param>
        public void KindOfResponse(String answer, int roomid, int reraiseValue)
        {
            var id = Context.ConnectionId;
            Room room = MvcApplication.rooms[roomid];
            Player player = room.Players.Where(p => p.Game.ConnectionId == id).FirstOrDefault();
            Bank bank = room.Bank.Where(b => b.IsBankCurrent == true).FirstOrDefault();

            switch(answer)
            {
                case "re-raise":
                    int reraise = reraiseValue;
                    bank.CurrentBank += reraise;
                    player.Game.CurrentBalance -= reraise;
                    player.Game.Bet += reraise;
                    player.Game.IsActive = false;
                    player.Game.IsWasMove = true;

                    if(player.Game.Bet > room.CurrentBet)
                        room.CurrentBet = player.Game.Bet;

                    Clients.Group(room.RoomId.ToString()).setBet(player, room.CurrentBet);
                    //передача хода следующему игроку, если он есть или следующий этап игры
                    MoveNextPlayer(room, player);

                    break;
                case "raise":
                    int raise = room.BigBlind;
                    room.CurrentBet += raise;

                    player.Game.CurrentBalance -= raise;
                    bank.CurrentBank += raise;
                    player.Game.Bet += raise;
                    player.Game.IsActive = false;
                    player.Game.IsWasMove = true;

                    Clients.Group(room.RoomId.ToString()).setBet(player, room.CurrentBet);
                    //передача хода следующему игроку, если он есть или следующий этап игры
                    MoveNextPlayer(room, player);

                    break;
                case "call":
                    int call = room.CurrentBet - player.Game.Bet;
                    player.Game.CurrentBalance -= call;
                    bank.CurrentBank += call;
                    player.Game.Bet += call;
                    player.Game.IsActive = false;
                    player.Game.IsWasMove = true;

                    Clients.Group(room.RoomId.ToString()).setBet(player, room.CurrentBet);
                    //передача хода следующему игроку, если он есть или следующий этап игры
                    MoveNextPlayer(room, player);

                    break;
                case "check":
                    player.Game.IsActive = false;
                    player.Game.IsWasMove = true;

                    //передача хода следующему игроку, если он есть или следующий этап игры
                    MoveNextPlayer(room, player);

                    break;
                case "fold":
                    //данны игрок больше не претендует на банк в течении данной игры
                    bank.CurrentPlayers.Remove(bank.CurrentPlayers.Where(p => p.Id == player.Id).FirstOrDefault());
                    player.Game.IsActive = false;
                    player.Game.IsPlayNow = false;
                    player.Game.IsFold = true;
                    player.Game.IsWasMove = true;

                    //передача хода следующему игроку, если он есть или следующий этап игры
                    MoveNextPlayer(room, player);

                    break;
                case "all-in":              
                    int allin = player.Game.CurrentBalance;
                    bank.CurrentBank += allin;
                    player.Game.Bet += allin;
                    player.Game.CurrentBalance = 0;
                    player.Game.IsActive = false;
                    player.Game.IsWasMove = true;

                    if (player.Game.Bet > room.CurrentBet)
                        room.CurrentBet = player.Game.Bet;

                    Clients.Group(room.RoomId.ToString()).setBet(player, room.CurrentBet);
                    //передача хода следующему игроку, если он есть или следующий этап игры
                    MoveNextPlayer(room, player);

                    break;
            }
        }

        //создание нового банка если общих карт меньше 5, и при этом хотябы один игрок сказал all-in, и при этом играющих больше одного кроме игроков сказавших all-in
        private void CreateNewBank(Room room)
        {
            Bank bank = room.Bank.Where(b => b.IsBankCurrent == true).FirstOrDefault();
            int playNow = bank.CurrentPlayers.Count(p => p.Game.IsPlayNow == true);
            int allin = bank.CurrentPlayers.Count(p => p.Game.IsPlayNow == true && p.Game.CurrentBalance == 0);
            //пример: играют 9 игроков. 2-е сказали all-in, остальные 7 сказали call и у них баланс больше 0 - для них создается следующий банк
            if (playNow > 1 && allin != 0)
            {
                Bank nextBank = new Bank();
                foreach (Player player in bank.CurrentPlayers)
                    nextBank.CurrentPlayers.Add(player);
                bank.IsBankCurrent = false;
                room.Bank.Add(nextBank);
            }
        }

        /// <summary>
        /// Передача ходу следующему игроку если он есть или следующий этап игры
        /// </summary>
        /// <param name="room">Комната</param>
        /// <param name="player">Игрок, от которого идет проверка</param>
        private void MoveNextPlayer(Room room, Player player)
        {
            //проверка на то сколько игроков играют, если из активных остался один - он выиграл
            int playersWhoPlay = room.Players.Count(p => p.Game.IsPlayNow == true);
            if(playersWhoPlay == 1)
            {
                //выигрывает игрок, который имеет значение IsPlayNow == true
                NextStageOrFinishGame(room, player, true);
                return;
            }

            //количество игроков которые в данный момент играют
            int PlayNow = room.Players.Count(p => p.Game.IsPlayNow == true);
            //количество игроков которые сказали свое слово хотябы 1 раз 
            int WasMove = room.Players.Count(p => p.Game.IsWasMove == true);
            //если значения совпадают - проверка на равенство ставок
            if(PlayNow == WasMove)
            {
                int SameBets = room.Players.Count(p => p.Game.Bet == room.CurrentBet && p.Game.IsPlayNow && p.Game.IsWasMove);
                //если ставки уравнены - следующий этап игры
                if(PlayNow == SameBets)
                {
                    NextStageOrFinishGame(room, player, false);
                    return;
                }
            }

            //--------------------------------------------------------------------------
            bool IsNextPlayer = false;
            //place = следующий игрок за столом после текущего
            for(int place = player.Game.Place + 1; place < room.Places.Length; place++)
            {
                Player nextPlayer = room.PlayerByPlace(place);
                if (nextPlayer == null)
                    continue;

                //если игрок подсел за стол во время игры или сказал Fold или all-in и у него баланс теперь равен 0, то ищем слледующего
                if (!nextPlayer.Game.IsPlayNow || nextPlayer.Game.IsFold || nextPlayer.Game.CurrentBalance == 0)
                    continue;

                nextPlayer.Game.IsActive = true;
                Clients.Client(nextPlayer.Game.ConnectionId).activation(true, nextPlayer, room.CurrentBet);
                IsNextPlayer = true;
                break;
            }

            //если нужно передать ход первому игроку сидящему сначала
            if(!IsNextPlayer)
            {
                for(int place = 0; place < player.Game.Place; place++)
                {
                    Player nextPlayer = room.PlayerByPlace(place);
                    if (nextPlayer == null)
                        continue;

                    //если игрок подсел за стол во время игры или сказал Fold или all-in и у него баланс теперь равен 0, то ищем слледующего
                    if (!nextPlayer.Game.IsPlayNow || nextPlayer.Game.IsFold || nextPlayer.Game.CurrentBalance == 0)
                        continue;

                    nextPlayer.Game.IsActive = true;
                    Clients.Client(nextPlayer.Game.ConnectionId).activation(true, nextPlayer, room.CurrentBet);
                    IsNextPlayer = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Переход на новый этап игры или проверка на победителя если уже роздано 5 общих карт
        /// </summary>
        /// <param name="room">Комната</param>
        /// <param name="player">Игрок, который последним сделал ход</param>
        /// <param name="lastPlayer">Если играющих игроков всего 1 - он выигрывает</param>
        private void NextStageOrFinishGame(Room room, Player player, bool lastPlayer)
        {
            //если играющий игрок на один
            if (!lastPlayer)
            {
                //если в течении этапа были сделаны ставки
                if (room.CurrentBet != 0)
                {
                    Clients.Group(room.RoomId.ToString()).collectBets();
                    Thread.Sleep(delayBets);
                }

                room.CurrentBet = 0;
                int lastPlayers = room.Players.Count(p => p.Game.CurrentBalance > 0); //если все игроки сказали all-in и остался один играющий с балансом больше нуля
                //на новом этапе все должны сделать минимум по 1-му ходу
                foreach (Player p in room.Players)
                {
                    //пропускать игрока, который на играет в данной игре или сделал all-in и имеет баланс 0
                    if (!p.Game.IsPlayNow)
                        continue;

                    if(p.Game.CurrentBalance == 0)
                    {
                        p.Game.Bet = 0;
                        continue;
                    }

                    if(lastPlayers == 1)
                    {
                        p.Game.Bet = 0;
                        continue;
                    }

                    p.Game.IsWasMove = false;
                    p.Game.Bet = 0;
                    //игрок, макс. банк в комнате, количество банков комнате, баланс в активном банке
                    Clients.Group(room.RoomId.ToString()).setBet(p, room.CurrentBet, room.Bank.Count, room.Bank.Where(b => b.IsBankCurrent == true).FirstOrDefault().CurrentBank);
                }

                //если все сказали all-in но еще не все карты розданы - открыть карты
                int countIsPlay = room.Players.Count(p => p.Game.IsPlayNow);
                int countAllin = room.Players.Count(p => p.Game.CurrentBalance == 0 && p.Game.IsPlayNow && p.Game.IsWasMove);
                if(countIsPlay == countAllin)
                {
                    foreach (Player p in room.Players)
                    {
                        //не показывать карты если игрок наблюдающий или в течении игры сказал fold
                        if (!p.Game.IsPlayNow || p.Game.IsFold)
                            continue;

                        Clients.Group(room.RoomId.ToString()).setBet(p, room.CurrentBet, room.Bank.Count, room.Bank.Where(b => b.IsBankCurrent == true).FirstOrDefault().CurrentBank);
                        Clients.Group(room.RoomId.ToString()).openCards(p);
                    }
                }

                //создается новый банк для играющих, если это необходимо
                CreateNewBank(room);
              
                int cardsInRoom = room.Cards.Count;
                if(cardsInRoom < 3)
                {
                    //раздача 3-х карт
                    for (int i = 0; i < 3; i++)
                    {
                        Thread.Sleep(1000);

                        Tools.Card card = room.Deck.Pop();
                        room.Cards.Add(card);

                        Clients.Group(room.RoomId.ToString()).animatedCard(null, card);
                    }

                    Thread.Sleep(delay);
                    MoveNextPlayer(room, player);
                    return;
                }
                else if(cardsInRoom >= 3 && cardsInRoom < 5)
                {
                    Thread.Sleep(1000);

                    Tools.Card card = room.Deck.Pop();
                    room.Cards.Add(card);

                    Clients.Group(room.RoomId.ToString()).animatedCard(null, card);

                    Thread.Sleep(delay);
                    MoveNextPlayer(room, player);
                    return;
                }
                else //если уже роздано 5 общих карт - выясняется победитель
                {
                    foreach (Player p in room.Players)
                    {
                        //не показывать карты если игрок наблюдающий или в течении игры сказал fold
                        if (!p.Game.IsPlayNow || p.Game.IsFold)
                            continue;

                        Clients.Group(room.RoomId.ToString()).openCards(p);
                    }
                    EndOfGame(room, player, false);
                }
            }
            else //если один - он выигрывает
            {
                if (room.CurrentBet != 0)
                {
                    Clients.Group(room.RoomId.ToString()).collectBets();
                    Thread.Sleep(delayBets);
                }

                EndOfGame(room, player, true);
            }
        }

        //завершение игры
        private void EndOfGame(Room room, Player player, bool islast)
        {
            if(islast)
            {
                //оставшийся игрок забирает все банки
                foreach(Bank bank in room.Bank)
                {
                    foreach(Player p in bank.CurrentPlayers)
                        if(p.Game.IsPlayNow)
                        {
                            foreach (Player realPlayer in room.Players)
                            {
                                if (realPlayer.Id != p.Id)
                                    continue;

                                realPlayer.Game.IsPlayNow = false;
                                int win = bank.CurrentBank;
                                realPlayer.Game.CurrentBalance += win;

                                Clients.Group(room.RoomId.ToString()).result(realPlayer, bank.Index, win, null, null);
                                Thread.Sleep(delay + 1000);
                                Clients.Group(room.RoomId.ToString()).clearPage();
                                break;
                            }
                            break;                   
                        }
                }
                room.IsGameOn = false;
                IsBeginAgain(room);               
                return;
            }

            //вычисление победителя из игроков которые претиндуют на каждый банк
            List<Tools.Card> roomCards = Tools.Deck.SortCards(room.Cards);
            foreach(Bank bank in room.Bank)
            {
                //игроки которые выиграли данный банк
                List<Player> Winners = new List<Player>();
                List<Player> AllPlayers = new List<Player>();
                foreach(Player p in bank.CurrentPlayers)
                {
                    List<Tools.Card> bestHand = new List<Tools.Card>();
                    foreach (Tools.Card c in roomCards)
                        bestHand.Add(c);

                    //замещение одной карты из общих пяти на одну из карт игрока
                    for(int i = 0; i < 2; i++)
                    {
                        for(int j = 0; j < 5; j++) //5 раз замена одной карты игрока на одну карту из общих
                        {
                            List<Tools.Card> curDeck = new List<Tools.Card>();
                            for(int k = 0; k < 5; k++)
                            {
                                if (j == k)
                                    curDeck.Add(p.Game.Cards.ToList()[i]);
                                else
                                    curDeck.Add(roomCards[k]);
                            }
                            curDeck = Tools.Deck.SortCards(curDeck);
                            int best = Tools.Deck.GetCombination(bestHand);
                            int cur = Tools.Deck.GetCombination(curDeck);
                            //если текущая комбинация больше лучшей
                            if (cur > best)
                                bestHand = curDeck;
                            else if(cur == best) //если комбинации одинаковые
                            {
                                bool IsBest = Tools.Deck.IsCurrentBest(bestHand, curDeck);
                                if (IsBest)
                                    bestHand = curDeck;
                            }
                        }
                    }
                    //замещение двух карт из общих пяти на две из карт игрока
                    for (int j = 0; j < 5; j++)
                    {
                        List<Tools.Card> curDeck = new List<Tools.Card>();
                        if(j < 4)
                        {
                            int k = j - 1;
                            if (k >= 0)
                                for (int i = 0; i <= k; i++)
                                    curDeck.Add(roomCards[i]);

                            curDeck.Add(p.Game.Cards.ToList()[0]);
                            curDeck.Add(p.Game.Cards.ToList()[1]);
                            for (int i = j + 2; i < 5; i++)
                                curDeck.Add(roomCards[i]);
                        }
                        else
                        {
                            curDeck.Add(p.Game.Cards.ToList()[0]);
                            curDeck.Add(roomCards[1]);
                            curDeck.Add(roomCards[2]);
                            curDeck.Add(roomCards[3]);
                            curDeck.Add(p.Game.Cards.ToList()[1]);                                                       
                        }
                        curDeck = Tools.Deck.SortCards(curDeck);
                        int best = Tools.Deck.GetCombination(bestHand);
                        int cur = Tools.Deck.GetCombination(curDeck);
                        //если текущая комбинация больше лучшей
                        if (cur > best)
                            bestHand = curDeck;
                        else if (cur == best) //если комбинации одинаковые
                        {
                            bool IsBest = Tools.Deck.IsCurrentBest(bestHand, curDeck);
                            if (IsBest)
                                bestHand = curDeck;
                        }
                    }
                    //запоминание лучшей комбинации из всех вариантов у данного игрока
                    p.Game.BestHand = bestHand;
                    AllPlayers.Add(p);
                }
                //отсеевание проигравших
                for (int i = 0; i < AllPlayers.Count; i++)
                {
                    for(int j = i + 1; j < AllPlayers.Count; j++)
                    {
                        int playerI = Tools.Deck.GetCombination(AllPlayers[i].Game.BestHand);
                        int playerJ = Tools.Deck.GetCombination(AllPlayers[j].Game.BestHand);
                        if (playerJ > playerI)
                        {
                            if (Winners.Contains(AllPlayers[i]))
                                Winners.Remove(AllPlayers[i]);

                            if (!Winners.Contains(AllPlayers[j]))
                                Winners.Add(AllPlayers[j]);
                        }
                        else if(playerI > playerJ)
                        {
                            if (Winners.Contains(AllPlayers[j]))
                                Winners.Remove(AllPlayers[j]);

                            if (!Winners.Contains(AllPlayers[i]))
                                Winners.Add(AllPlayers[i]);
                        }
                        else if (playerJ == playerI)
                        {
                            int valueI = AllPlayers[i].Game.BestHand.Sum(c => c.Value);
                            int valueJ = AllPlayers[j].Game.BestHand.Sum(c => c.Value);
                            if(valueI > valueJ)
                            {
                                if (Winners.Contains(AllPlayers[j]))
                                    Winners.Remove(AllPlayers[j]);

                                if (!Winners.Contains(AllPlayers[i]))
                                    Winners.Add(AllPlayers[i]);
                            }
                            else if(valueJ > valueI)
                            {
                                if (Winners.Contains(AllPlayers[i]))
                                    Winners.Remove(AllPlayers[i]);

                                if (!Winners.Contains(AllPlayers[j]))
                                    Winners.Add(AllPlayers[j]);
                            }
                            else
                            {
                                if (!Winners.Contains(AllPlayers[i]))
                                    Winners.Add(AllPlayers[i]);

                                if (!Winners.Contains(AllPlayers[j]))
                                    Winners.Add(AllPlayers[j]);
                            }
                        }
                    }
                }
                //отправка результатов
                foreach(Player p in Winners)
                {
                    foreach (Player realPlayer in room.Players)
                        if (realPlayer.Id == p.Id)
                        {
                            int win = bank.CurrentBank / Winners.Count;
                            realPlayer.Game.CurrentBalance += win;

                            realPlayer.Game.IsWinGame = true;
                            realPlayer.Game.Gain = win;

                            Clients.Group(room.RoomId.ToString()).result(realPlayer, bank.Index, win, realPlayer.Game.BestHand, Tools.Deck.CombinationName(Tools.Deck.GetCombination(realPlayer.Game.BestHand)));
                            break;
                        }
                }
                Thread.Sleep(delay + 2000);
            }

            //игра начинается заново
            room.IsGameOn = false;
            if (room.RoomGame == "Texas Holdem Poker")
                BeginPlayHoldem(room);
        }

        /// <summary>
        /// Начало раздачи карт игрокам начиная с количества: 2 игрока
        /// </summary>
        private void BeginPlayHoldem(Room room)
        {
            //если игра уже идет
            if (room.IsGameOn)
                return;
            
            //очистка страницы перед началом новой игры
            Clients.Group(room.RoomId.ToString()).clearPage();

            //если это не первая раздача - сохранения данных по игре
            room.SaveGame();

            //отображение для каждого игрока своего exp
            foreach(Player player in room.Players)
            {
                if (player.Game.Place == -1)
                    continue;
                if (!player.Game.IsPlayNow && !player.Game.IsFold)
                    continue;
                if (player.Game.Exp == 0)
                    continue;

                Clients.Client(player.Game.ConnectionId).showExp(player.Game.Exp);
            }

            room.ResetData();

            foreach(Player player in room.Players)
            {
                //если игрок только наблюдает за игрой
                if (player.Game.Place == -1)
                    continue;

                if (player.Game.CurrentBalance < room.BigBlind)
                {
                    player.Game.IsInsolvent = true;
                    Clients.Client(player.Game.ConnectionId).lostPlace();
                }
            }

            //если игроков за столом способных делать минимальные ставки меньше 2-х
            int sitCount = room.Players.Count(p => p.Game.Place != -1 && p.Game.CurrentBalance >= room.BigBlind);
            if (sitCount < 2)
                return;

            Thread.Sleep(2000);

            room.IsGameOn = true;
            foreach(Player player in room.Players)
            {
                if (player.Game.Place == -1)
                    continue;

                player.Game.ResetData();
            }

            int placeOfBigBlind = -1; //отдать ход следующему после этого места
            //передача хода
            if(room.CurPlaceBigBlind == -1) //если раздача выполняется впервые
            {
                bool smallBlind = false;
                for(int place = 0; place < room.Places.Length; place++)
                {
                    Player player = room.PlayerByPlace(place);
                    if (player == null)
                        continue;

                    if (player.Game.IsInsolvent)
                        continue;

                    if(!smallBlind)
                    {
                        player.Game.IsSmallBlind = true;
                        smallBlind = true;
                    }
                    else
                    {
                        player.Game.IsBigBlind = true;
                        room.CurPlaceBigBlind = place;
                        placeOfBigBlind = place;
                        break;
                    }
                }
            }
            else //если это не первая раздача в комнате
            {
                bool bigBlind = false;
                for(int place = 0; place < room.Places.Length; place++)
                {
                    if (room.CurPlaceBigBlind > place)
                        continue;

                    Player player = room.PlayerByPlace(place);
                    if (player == null)
                        continue;

                    if (player.Game.IsInsolvent)
                        continue;

                    //игрок, у которого был большой блайнд получает малый
                    if (room.CurPlaceBigBlind == place)
                    {
                        player.Game.IsBigBlind = false;
                        player.Game.IsSmallBlind = true;
                        continue;
                    }

                    player.Game.IsBigBlind = true;
                    bigBlind = true;
                    room.CurPlaceBigBlind = place;
                    placeOfBigBlind = place;
                    break;
                }

                //если раздача большого блайнда должна идти сначала
                if(!bigBlind)
                {
                    for (int place = 0; place < room.Places.Length; place++)
                    {
                        Player player = room.PlayerByPlace(place);
                        if (player == null)
                            continue;

                        if (player.Game.IsInsolvent)
                            continue;

                        //большой блайнд получает первый игрок сначала
                        player.Game.IsBigBlind = true;
                        room.CurPlaceBigBlind = place;
                        placeOfBigBlind = place;
                        break;
                    }
                }
            }

            //создание первого банка для текущей игры для всех игроков которые ожидают раздачи карт
            Bank firstBank = new Bank();
            firstBank.Index = 1;
            firstBank.CurrentPlayers = room.Players.Where(p => p.Game.IsPlayNow == true).ToList();
            
            foreach(Player player in room.Players)
            {
                if (!player.Game.IsSmallBlind && !player.Game.IsBigBlind)
                    continue;

                if (player.Game.IsInsolvent)
                    continue;

                int bet = 0;
                if (player.Game.IsSmallBlind) //если игрок ставит малый блайнд
                    bet = room.SmallBlind;
                else    //если игрок ставит большой блайнд
                {
                    bet = room.BigBlind;
                    room.CurrentBet = bet;
                }

                player.Game.CurrentBalance -= bet;
                player.Game.Bet += bet;
                firstBank.CurrentBank += bet;

                Clients.Group(room.RoomId.ToString()).setBet(player);
            }

            room.Bank.Add(firstBank);
            Thread.Sleep(delay);

            //раздача по 2 карты
            for (int i = 0; i < 2; i++)
            {
                for (int place = 0; place < room.Places.Length; place++)
                {
                    Player player = room.PlayerByPlace(place);
                    if (player == null)
                        continue;

                    if (player.Game.IsInsolvent)
                        continue;

                    Thread.Sleep(delayCard);

                    Tools.Card card = room.Deck.Pop();
                    player.Game.Cards.Push(card);

                    Clients.Group(room.RoomId.ToString()).animatedCard(player, card);
                }
            }

            Thread.Sleep(delay);

            //активация игрока, который сейчас должен делать ход
            bool isActive = false;
            for(int place = 0; place < room.Places.Length; place++)
            {
                Player player = room.PlayerByPlace(place);
                if (player == null)
                    continue;

                if (player.Game.IsInsolvent)
                    continue;

                if (player.Game.Place > placeOfBigBlind)
                {
                    player.Game.IsActive = true;
                    isActive = true;
                    Clients.Client(player.Game.ConnectionId).activation(true, player, room.CurrentBet);
                    break;
                }
            }

            //если ход нужно передать первому игроку сначала 
            if(!isActive)
            {
                for (int place = 0; place < room.Places.Length; place++)
                {
                    Player player = room.PlayerByPlace(place);
                    if (player == null)
                        continue;

                    if (player.Game.IsInsolvent)
                        continue;

                    player.Game.IsActive = true;
                    Clients.Client(player.Game.ConnectionId).activation(true, player, room.CurrentBet);
                    break;
                }
            }
        }

        /// <summary>
        /// Если досрочно выиграл один игрок и при этом игроков за столом больше одного - начинается новая игра
        /// </summary>
        /// <param name="room"></param>
        private void IsBeginAgain(Room room)
        {
            int sit = room.Players.Count(player => player.Game.Place != -1);
            if (sit > 1)                
                BeginPlayHoldem(room);
        }

        // Подключение нового пользователя
        public override Task OnConnected()
        {
            var id = Context.ConnectionId;
            var userID = Context.User.Identity.GetUserId();
            String roomID = "";
            
            foreach(Room room in MvcApplication.rooms)
                foreach(Player player in room.Players)
                {
                    if(player.Id == userID)
                    {
                        player.Game.ConnectionId = id;
                        roomID = room.RoomId.ToString();
                        break;
                    }
                }

            Groups.Add(id, roomID);

            return base.OnConnected();
        }

        // Отключение пользователя
        public override Task OnDisconnected(bool stopCalled)
        {
            var userID = Context.User.Identity.GetUserId();
            Room r = null;
            Player p = null;
            bool isMove = false;

            foreach(Room room in MvcApplication.rooms)
                foreach(Player player in room.Players)
                    if(player.Id == userID)
                    {
                        //если вышел игрок у которого сейчас был ход
                        if (player.Game.IsActive)
                            isMove = true;

                        if (player.Game.Place != -1)
                            room.Places[player.Game.Place] = 0;

                        try
                        {
                            Player pp = db.Players.Find(player.Id);
                            pp.Balance += player.Game.CurrentBalance;
                            db.SaveChanges();
                        }
                        catch { }

                        p = player;
                        room.Players.Remove(player);
                        r = room;
                        break;
                    }

            Clients.OthersInGroup(p.Game.RoomId.ToString()).removePlayer(p);
            p.Game = new GameInfo();

            foreach(Bank bank in r.Bank)
                foreach(Player player in bank.CurrentPlayers)
                    if(player.Id == p.Id)
                    {
                        bank.CurrentPlayers.Remove(player);
                        break;
                    }   

            int sitCount = r.Players.Count(pl => pl.Game.Place != -1 && pl.Game.IsPlayNow == true);
            if (sitCount < 2)
            {
                EndOfGame(r, null, true);
                r.IsGameOn = false;
                r.CurPlaceBigBlind = -1;
            }
            else if(isMove)
                MoveNextPlayer(r, p);
            else
            {
                if (r.Players.Count(pl => pl.Game.Place != -1) == 1)
                {
                    r.IsGameOn = false;
                    r.CurPlaceBigBlind = -1;
                }
            }

            return base.OnDisconnected(stopCalled);
        }
    }
}