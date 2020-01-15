package scxmlgen.Modalities;

import scxmlgen.interfaces.IModality;

/**
 *
 * @author nunof
 */
public enum SecondMod implements IModality{

    EsvaziarC("[1][esvaziarC]",3000),
    RecuarR("[1][RecuarR]",3000),
	AvancarL("[0][AvancarL]",3000),
    scrollU("[4][ScrollU]",3000),
	scrollDR("[3][scrollDR]",3000);
    
    private String event;
    private int timeout;


    SecondMod(String m, int time) {
        event=m;
        timeout=time;
    }

    @Override
    public int getTimeOut() {
        return timeout;
    }

    @Override
    public String getEventName() {
        //return getModalityName()+"."+event;
        return event;
    }

    @Override
    public String getEvName() {
        return getModalityName().toLowerCase()+event.toLowerCase();
    }
    
}
