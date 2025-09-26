def creat():
    pass

def delete():
    pass

def search():
    pass

def close():
    pass

def show():
    pass

def exit():
    print("Closing programm...")
def interface():
    print("Hello, dear user! A'm manager of your notes.")
    while True:
        print('''There are commands you can use:
              1 - creat note
              2 - delete note
              3 - search for note
              4 - close note
              5 - show all notes
              6 - close programm
              Please enter the number of command you want to use: ''')
        answer = input()
        match answer:
            case "1":
               creat() 
            case "2":
               delete() 
            case "3":
               search() 
            case "4":
               close() 
            case "5":
               show() 
            case "6":
                exit()
                break
            case _:
                print("Please check your input. I don't thing there is a command with this number.")
                continue

file = open("Notes.txt", "r+")
interface()