import React, { Component } from 'react';
import { HubConnectionBuilder, LogLevel } from '@aspnet/signalr'

import GameJoin from '../components/GameJoin'
import GameCanvas from '../components/GameCanvas'
import GameVoting from '../components/GameVoting';

class Play extends Component {
        
    socket = null;

    constructor(props) {
        super(props);

        this.state = {
            hubConnection: null,
            color: '#000000',
            word: '',
            canvasVisible: false,
            voteVisible: false,
            joinVisible: true,
            disabledCanvas:true,
            gamecode: '',
            username: '',
            voters: [],
            currentSketch: [],
            items: []
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
                        console.log('Connection started!');
                        //this.state.hubConnection.invoke('updateuser', user.email, user.name);
                    })
                .catch(err => console.log('Error while establishing connection :('));

            this.state.hubConnection.on('renderBoard', board => {
                this.setState({ board: board })
            });
            this.state.hubConnection.on('joined', player => {
                this.setState({ message: "Welcome, waiting for others...", color: player.color, yourTurn: false, showButtons: false, canvasVisible: true, joinVisible: false, disabledCanvas: false })
            });
            this.state.hubConnection.on('readyplayer', player => {
                this.setState({ color: player.color, word: player.word })
            });
            this.state.hubConnection.on('turn', player => {
                if (player.color === this.state.color) {
                    this.setState({ message: "Your Turn. Start Drawing.", yourTurn: true, showButtons: false, canvasVisible: true, joinVisible: false, disabledCanvas: false })
                } else {
                    this.setState({ message: player.name + ' is drawing...', yourTurn: false, showButtons: false, canvasVisible: true, joinVisible: false, disabledCanvas: true })
                }
            });
            this.state.hubConnection.on('updateCanvas', (items) => {
                this.setState({ items: this.state.items.concat(items) });
                this.setState({ currentSketch: [] }); // Clear Temp Sketch Object
            });

            this.state.hubConnection.on('timesupsent', () => {
                this.state.hubConnection.invoke('submitdrawing', this.state.currentSketch).catch(err => console.error(err.toString()));
            });

            this.state.hubConnection.on('round2start', () => {
                this.setState({ message: 'Round 2 Start' });
            });
            this.state.hubConnection.on('votinground', (voters) => {
                this.setState({ message: 'Find the Faker...', voters: voters, voteVisible: true, yourTurn: false, canvasVisible: false, disabledCanvas: true  });
            });

            this.state.hubConnection.on('concede', () => {
                this.setState({ message: 'Your opponent conceded. You win', yourTurn: false, showButtons: true })
                this.state.hubConnection.stop()
            });

            this.state.hubConnection.on('victory', (player, board) => {
                let newState = { yourTurn: false }
                if (player === this.state.color) {
                    newState['message'] = 'You win!'
                } else {
                    newState['message'] = 'You lose!'
                }
                newState["board"] = board;
                newState["showButtons"] = true;
                this.setState(newState)
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

    mySubmitHandler = (event) => {
        event.preventDefault();
        this.state.hubConnection.invoke('join', this.state.gamecode, this.state.username).catch(err => console.error(err.toString()));
    }

    myChangeHandler = (event) => {
        let nam = event.target.name;
        let val = event.target.value;
        this.setState({ [nam]: val });
    } 

    onCompleteItem = (i) => {
        this.setState({ currentSketch: this.state.currentSketch.concat(i) });
    }

    submitDrawing = (event) => {
        event.preventDefault();
        this.state.hubConnection.invoke('submitdrawing', this.state.currentSketch).catch(err => console.error(err.toString()));
    }

    submitVote = (connectionid) => {
        this.state.hubConnection.invoke('submitVote', connectionid).catch(err => console.error(err.toString()));
    }

    render() {
        const { tool, size, color, fill, fillColor, items } = this.state;
        return (
            <div>
                <h1>Play</h1>
                <div className="gamecode" style={{ visibility: this.state.joinVisible ? 'hidden' : 'visible' }}>{this.state.gamecode}</div>
                <div style={{ 'background-color': this.state.color, visibility: this.state.joinVisible ? 'hidden' : 'visible' }} className="player-name">{this.state.username}</div>
                <div className="player-message">{this.state.message}</div>
                <div className="player-word" style={{ visibility: !this.state.canvasVisible ? 'hidden' : 'visible' }}>{this.state.word}</div>
                <GameJoin visible={this.state.joinVisible} mySubmitHandler={this.mySubmitHandler} myChangeHandler={this.myChangeHandler}  />
                <GameCanvas color={this.state.color} visible={this.state.canvasVisible} items={this.state.items} submitDrawing={this.submitDrawing} onCompleteItem={this.onCompleteItem} disabled={this.state.disabledCanvas} />
                <GameVoting visible={this.state.voteVisible} voters={this.state.voters} color={this.state.color} submitVote={this.submitVote} />
            </div>
        );
    }
}

export default Play;
