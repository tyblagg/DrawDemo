const React = require('react')

class GameTimer extends React.Component {
    constructor(props) {
        super(props)

        this.state = {
            seconds: '00',   // responsible for the seconds 
            minutes: ''  // responsible for the minutes
        }

        this.secondsRemaining = 0;
        this.intervalHandle = null;

        // method that triggers the countdown functionality
        this.handleChange = this.handleChange.bind(this);        
        this.startCountDown = this.startCountDown.bind(this);
        this.tick = this.tick.bind(this);
    }

    componentDidUpdate(prevProps) {
        if (prevProps.isOn !== this.props.isOn) {
            if (this.props.isOn) {
                this.startCountDown();
            }            
        }
    }

    handleChange(event) {
        this.setState({
            minutes: event.target.value
        })
    }

    tick() {
        var min = Math.floor(this.secondsRemaining / 60);
        var sec = this.secondsRemaining - (min * 60);

        this.setState({
            minutes: min,
            seconds: sec
        })

        if (sec < 10) {
            this.setState({
                seconds: "0" + sec,
            })
        }

        if (min < 10) {
            this.setState({
                value: "0" + min,
            })
        }

        if (min === 0 & sec === 0) {
            clearInterval(this.intervalHandle);
            this.props.timerEndHandler();
        }

        this.secondsRemaining--
    }

    startCountDown() {
        this.intervalHandle = setInterval(this.tick, 1000);
        let time = this.props.minutes;
        this.secondsRemaining = time * 60;
        this.secondsRemaining += this.props.seconds;
    }

    render() {        
        return (
            <div>
                <div>{this.state.minutes}:{this.state.seconds}</div>
            </div>
        )
    }
}

export default GameTimer