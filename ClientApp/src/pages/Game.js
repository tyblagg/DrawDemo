import React, { Component } from 'react';
import SketchPad from '../store/SketchPad';
import { TOOL_PENCIL } from '../tools'
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr'
import GameTimer from '../components/GameTimer';

class Game extends Component {
        
    socket = null;

    constructor(props) {
        super(props);

        this.state = {
            hubConnection: null,
            connectCode: '',
            category: '',
            tool: TOOL_PENCIL,
            size: 2,
            color: '#000000',
            fill: true,
            fillColor: '#444444',
            items: [],
            isOn: false,
            seconds: '00',
            minutes: ''
        }
    }

    
    componentDidMount() {

        const hubConnection = new HubConnectionBuilder().withUrl("/hub").configureLogging(LogLevel.Information).build();
        // This method is called when the component is first added to the document
        this.ensureDataFetched();

        this.setState({ hubConnection }, () => {
            this.state.hubConnection
                .start()
                .then(
                    () => {
                        this.state.hubConnection.invoke('create').catch(err => console.error(err.toString()));
                        console.log('Connection started!');
                        //this.state.hubConnection.invoke('updateuser', user.email, user.name);
                    })
                .catch(err => console.log('Error while establishing connection :('));

            this.state.hubConnection.on('updatecanvas', (items) => {
                this.setState({ items: this.state.items.concat(items) });
            });
            this.state.hubConnection.on('connectcode', connectcode => {
                this.setState({ connectCode: connectcode })
            });
            this.state.hubConnection.on('sendcategory', category => {
                this.setState({ category: category })
            });

            this.state.hubConnection.on('Waiting', (player) => {
                // TODO: Display Players List
                //this.setState({ message: player.name + ' Joined!' });
            });

            this.state.hubConnection.on('turn', player => {
                // TODO: Set Timer
                this.setState({ message: player.name + ' is drawing...', seconds: 15, isOn: true });
            });

            this.state.hubConnection.on('round2start', () => {
                this.setState({ message: 'Round 2 Start' });
            });
            this.state.hubConnection.on('votinground', () => {
                // TODO: Set Timer
                this.setState({ message: 'Find the Faker...', yourTurn: false });
            });

            this.state.hubConnection.on('concede', () => {
                this.setState({ message: 'Your opponent conceded. You win', yourTurn: false, showButtons: true })
                this.state.hubConnection.stop()
            });

            this.state.hubConnection.on('PlayersVotedFor', (player) => {
                var isFaker = player.word == "Fake It";
                var m = 'Players voted for ' + player.name + '. They were ' + (isFaker ? "the faker!!!" : "NOT the faker!!!");
                if (isFaker) {
                    m += ' Faker can you guess what the word was, to win?'
                }
                this.setState({ message: m, yourTurn: false, showButtons: true })
                this.state.hubConnection.stop()
            });

            this.state.hubConnection.on('VotesNotMet', (player) => {                
                this.setState({ message: 'A majority was not met. The Faker Wins!!!', yourTurn: false, showButtons: true })
                this.state.hubConnection.stop()
            });

        });
    }

    componentDidUpdate() {
        // This method is called when the route parameters change
        this.ensureDataFetched();
    }

    ensureDataFetched() {
        
    }

    startGameClick = (event) => {
        this.state.hubConnection.invoke('start').catch(err => console.error(err.toString()));
    } 

    timerEndHandler = () => {
        this.setState({ message: 'Time is up!', isOn: false });
        this.state.hubConnection.invoke('timesup').catch(err => console.error(err.toString()));
    }

    render() {
        const { tool, size, color, fill, fillColor, items } = this.state;
        return (
            <div>
                <h1>Game</h1>
                <button onClick={this.startGameClick}>Start</button>
                <p>{this.state.connectCode}</p>
                <p>{this.state.message}</p>
                <GameTimer minutes={this.state.minutes} seconds={this.state.seconds} isOn={this.state.isOn} timerEndHandler={this.timerEndHandler} />
                <p>{this.state.category}</p>
                <div style={{ pointerEvents: "none"}}>
                    <SketchPad
                        animate={true}
                        size={size}
                        color={color}
                        fillColor={fill ? fillColor : ''}
                        items={items}
                        tool={tool}
                    />
                </div>
            </div>
        );
    }
}

export default Game;
